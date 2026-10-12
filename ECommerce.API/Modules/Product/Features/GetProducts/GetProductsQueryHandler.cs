using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, ECommerce.API.Infrastructure.Models.PagedResult<ProductResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetProductsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ECommerce.API.Infrastructure.Models.PagedResult<ProductResponse>> HandleAsync(GetProductsQuery request, CancellationToken cancellationToken = default)
    {
        var statusKey = request.Status?.ToLower() ?? "active";
        
        var rawQuery = _dbContext.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .IgnoreQueryFilters();

        if (request.CanReadDeleted && (statusKey == "deleted" || statusKey == "all"))
        {
            if (statusKey == "deleted")
                rawQuery = rawQuery.Where(p => p.IsDeleted);
        }
        else
        {
            rawQuery = rawQuery.Where(p => !p.IsDeleted);
        }

        // Apply Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchStr = request.Search.ToLower();
            bool isGuid = Guid.TryParse(searchStr, out var parsedGuid);
            if (isGuid)
            {
                rawQuery = rawQuery.Where(p => p.Id == parsedGuid);
            }
            else
            {
                rawQuery = rawQuery.Where(p => p.Name.ToLower().Contains(searchStr));
            }
        }

        // Apply Category Filter
        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
        {
            rawQuery = rawQuery.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Apply Stock Status Filter
        if (!string.IsNullOrWhiteSpace(request.StockStatus) && request.StockStatus != "all")
        {
            if (request.StockStatus == "in_stock")
                rawQuery = rawQuery.Where(p => p.Stock > 0);
            else if (request.StockStatus == "low_stock")
                rawQuery = rawQuery.Where(p => p.Stock > 0 && p.Stock <= 5);
            else if (request.StockStatus == "out_of_stock")
                rawQuery = rawQuery.Where(p => p.Stock == 0);
        }

        // Apply Price Filter
        if (request.MinPrice.HasValue)
            rawQuery = rawQuery.Where(p => p.Price >= request.MinPrice.Value);
        
        if (request.MaxPrice.HasValue)
            rawQuery = rawQuery.Where(p => p.Price <= request.MaxPrice.Value);

        // Sorting
        bool isAsc = (request.SortOrder?.ToLower() ?? "desc") == "asc";
        var sortField = request.SortBy?.ToLower();

        rawQuery = sortField switch
        {
            "name" => isAsc ? rawQuery.OrderBy(p => p.Name) : rawQuery.OrderByDescending(p => p.Name),
            "price" => isAsc ? rawQuery.OrderBy(p => p.Price) : rawQuery.OrderByDescending(p => p.Price),
            "stock" => isAsc ? rawQuery.OrderBy(p => p.Stock) : rawQuery.OrderByDescending(p => p.Stock),
            "categoryid" => isAsc ? rawQuery.OrderBy(p => p.Category.Name) : rawQuery.OrderByDescending(p => p.Category.Name),
            _ => isAsc ? rawQuery.OrderBy(p => p.Id) : rawQuery.OrderByDescending(p => p.Id),
        };

        var totalCount = await rawQuery.CountAsync(cancellationToken);

        var items = await rawQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.IsDeleted,
                p.CategoryId,
                p.Category.Name,
                p.Images.Any(),
                p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder).Select(i => i.ThumbnailPath).FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);

        return new ECommerce.API.Infrastructure.Models.PagedResult<ProductResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
