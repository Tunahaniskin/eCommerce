using Microsoft.EntityFrameworkCore;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;

namespace ECommerce.API.Modules.Catalog.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog/products", async (AppDbContext dbContext) =>
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Select(p => new { p.Id, p.Name, p.Price, p.Stock })
                .ToListAsync();

            return Results.Ok(products);
        });
    }
}