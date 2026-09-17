using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // 1. Tekil Ürün Getirme (Get By Id)
        app.MapGet("/api/catalog/products/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var product = await dbContext.Products
                .AsNoTracking() // RAM'de entity tracking mekanizmasını kapatır, okumayı hızlandırır
                .Where(p => p.Id == id)
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock, p.ReservedStock)) // Projection
                .FirstOrDefaultAsync();

            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            return Results.Ok(product);
        });

        // 2. Tüm Ürünleri Listeleme (Get All)
        app.MapGet("/api/catalog/products", async (AppDbContext dbContext) =>
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock, p.ReservedStock))
                .ToListAsync();

            return Results.Ok(products);
        });
    }
}

// Entity'i dışarı sızdırmamak için kullanılan okuma modeli
public record ProductDto(Guid Id, string Name, decimal Price, int Stock, int ReservedStock);