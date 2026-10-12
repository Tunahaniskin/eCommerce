using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly AppDbContext _dbContext;

    public GetCategoriesQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CategoryDto>> HandleAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _dbContext.Categories;
        
        var totalCount = await dbQuery.CountAsync(cancellationToken);
        
        var categories = await dbQuery
            .Select(c => new 
            {
                c.Id,
                c.Name,
                c.Description,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                c.CreatedAt
            })
            .OrderBy(c => c.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ProductCount,
            c.CreatedAt
        )).ToList();
        
        return new PagedResult<CategoryDto>(items, totalCount, query.Page, query.PageSize);
    }
}
