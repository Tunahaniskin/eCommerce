using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Storage;
using ProductEntity = ECommerce.API.Modules.Product.Entities.Product;
using ECommerce.API.Modules.Product.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public CreateProductCommandHandler(AppDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<CreateProductResult> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var product = new ProductEntity(request.Name, request.Price, request.Stock, request.CategoryId);
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (request.Images != null && request.Images.Any())
            {
                var encoder = new WebpEncoder { Quality = 80 };
                
                foreach (var file in request.Images)
                {
                    if (file.Length == 0) continue;
                    if (file.Length > 5 * 1024 * 1024)
                        throw new ArgumentException($"{file.FileName} boyutu 5MB'ı aşamaz.");

                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    using var image = await Image.LoadAsync(memoryStream, cancellationToken);

                    if (image.Width > 1200)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1200, 0),
                            Mode = ResizeMode.Max
                        }));
                    }

                    using var thumbImage = image.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(200, 200),
                        Mode = ResizeMode.Max
                    }));

                    using var webpStream = new MemoryStream();
                    using var thumbWebpStream = new MemoryStream();

                    await image.SaveAsWebpAsync(webpStream, encoder, cancellationToken);
                    await thumbImage.SaveAsWebpAsync(thumbWebpStream, encoder, cancellationToken);

                    var imageData = webpStream.ToArray();
                    var thumbData = thumbWebpStream.ToArray();
                    
                    var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var imageFileName = $"{Guid.NewGuid()}_{originalFileName}.webp";
                    var thumbFileName = $"{Guid.NewGuid()}_{originalFileName}_thumb.webp";

                    var imagePath = await _fileStorage.SaveFileAsync(imageData, "uploads/products", imageFileName, cancellationToken);
                    var thumbPath = await _fileStorage.SaveFileAsync(thumbData, "uploads/products", thumbFileName, cancellationToken);

                    var isMain = product.Images.Count == 0;
                    var displayOrder = product.Images.Count;

                    var productImage = new ProductImage(
                        product.Id,
                        imageFileName,
                        "image/webp",
                        imagePath,
                        thumbPath,
                        imageData.Length,
                        isMain,
                        displayOrder
                    );

                    _dbContext.ProductImages.Add(productImage); // EXPLICITLY ADD TO DBSET
                    product.Images.Add(productImage); // Add to navigation property
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return new CreateProductResult(product.Id, "Ürün başarıyla oluşturuldu.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
