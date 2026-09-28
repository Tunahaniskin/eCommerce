using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly AppDbContext _dbContext;

    public DeleteProductCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteProductResult> HandleAsync(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return new DeleteProductResult(false, "Geçersiz ürün kimliği.");

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null || product.IsDeleted)
            return new DeleteProductResult(false, "Ürün bulunamadı.");

        product.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true, "Ürün başarıyla silindi.");
    }
}
