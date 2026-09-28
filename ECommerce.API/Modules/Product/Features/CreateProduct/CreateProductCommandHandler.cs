using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ProductEntity = ECommerce.API.Modules.Product.Entities.Product;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly AppDbContext _dbContext;

    public CreateProductCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProductResult> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = new ProductEntity(request.Name, request.Price, request.Stock);
        
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProductResult(product.Id, "Ürün başarıyla oluşturuldu.");
    }
}
