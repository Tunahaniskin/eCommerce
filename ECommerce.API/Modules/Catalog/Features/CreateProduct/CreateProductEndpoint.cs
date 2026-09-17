using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Modules.Catalog.Entities;

namespace ECommerce.API.Modules.Catalog.Features.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog/products", async (CreateProductRequest request, AppDbContext dbContext) =>
        {
            // 1. Validasyon Kontrolleri (Fail Fast)
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { Message = "Ürün adı boş olamaz." });

            if (request.Price <= 0)
                return Results.BadRequest(new { Message = "Fiyat sıfırdan büyük olmalıdır." });

            if (request.Stock < 0)
                return Results.BadRequest(new { Message = "Stok eksi değer alamaz." });

            // 2. Domain Nesnesini Oluşturma
            var product = new Product(request.Name, request.Price, request.Stock);
            
            // 3. Veritabanına Yazma
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            // 4. RESTful standardına uygun 201 Created dönme
            return Results.Created($"/api/catalog/products/{product.Id}", new 
            { 
                product.Id, 
                Message = "Ürün başarıyla oluşturuldu." 
            });
        });
    }
}

public record CreateProductRequest(string Name, decimal Price, int Stock);