using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDetailResponse?>
{
    private readonly AppDbContext _dbContext;

    public GetProductByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDetailResponse?> HandleAsync(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return null;

        if (product.IsDeleted && !request.CanReadDeleted) return null;

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Price,
            product.Stock,
            product.IsDeleted
        );
    }
}
