using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly AppDbContext _dbContext;

    public UpdateProductCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProductResult> HandleAsync(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new Exception("Ürün bulunamadı."); // A more specific exception like NotFoundException is better, but this matches typical early-stage setups
        }

        product.UpdateDetails(request.Name, request.Price, request.Stock, request.CategoryId);

        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(product.Id, "Ürün başarıyla güncellendi.");
    }
}
