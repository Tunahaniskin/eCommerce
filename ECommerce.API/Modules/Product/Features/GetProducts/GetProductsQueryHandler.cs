using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, List<ProductResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetProductsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductResponse>> HandleAsync(GetProductsQuery request, CancellationToken cancellationToken = default)
    {
        var statusKey = request.Status?.ToLower() ?? "active";
        IQueryable<Entities.Product> query;

        if (request.CanReadDeleted && (statusKey == "deleted" || statusKey == "all"))
        {
            var rawQuery = _dbContext.Products.AsNoTracking().IgnoreQueryFilters();
            query = statusKey == "deleted" ? rawQuery.Where(p => p.IsDeleted) : rawQuery;
        }
        else
        {
            query = _dbContext.Products.AsNoTracking();
        }

        query = query.OrderByDescending(p => p.Id); 

        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.IsDeleted
            ))
            .ToListAsync(cancellationToken);
    }
}
