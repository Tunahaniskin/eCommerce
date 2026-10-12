using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Product.Entities;
using ECommerce.API.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.UploadImage;

public class UploadImageCommandHandler : ICommandHandler<UploadImageCommand, List<UploadedImageDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public UploadImageCommandHandler(AppDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<List<UploadedImageDto>> HandleAsync(UploadImageCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Ürün bulunamadı.");

        var uploadedDtos = new List<UploadedImageDto>();
        var encoder = new WebpEncoder { Quality = 80 };

        foreach (var file in command.Files)
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
            product.Images.Add(productImage); // Also add to collection for correct display order on next loop iteration
            
            uploadedDtos.Add(new UploadedImageDto(productImage.Id, productImage.IsMain, productImage.DisplayOrder));
        }

        try { await _dbContext.SaveChangesAsync(cancellationToken); }         
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.FirstOrDefault();
            var entityType = entry?.Entity?.GetType()?.Name ?? "Unknown";
            var state = entry?.State.ToString() ?? "Unknown";
            throw new InvalidOperationException($"Concurrency Error on {entityType} (State: {state}). Message: {ex.Message}", ex);
        }

        return uploadedDtos;
    }
}

