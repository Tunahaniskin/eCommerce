using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.CreateProduct;

// 1. Dışarıdan gelecek olan verinin şeması (DTO / Request)
public record CreateProductRequest(string Name, decimal Price, int Stock);

// 2. HTTP İsteğini karşılayan ve iş mantığını çalıştıran sınıf
public static class CreateProductEndpoint
{
    // IEndpointRouteBuilder, Minimal API rotalarını modüler olarak tanımlamamızı sağlar
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog/products", async (CreateProductRequest request, AppDbContext dbContext) =>
        {
            // a. Domain nesnesini (Entity) oluşturuyoruz. Kurallar Product içindeki constructorda çalışır.
            var product = new Product(request.Name, request.Price, request.Stock);

            // b. Veritabanına ekliyor ve kaydediyoruz.
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            // c. İşlem başarılı (HTTP 200 OK) dönüyoruz.
            return Results.Ok(new { product.Id, Message = "Ürün başarıyla oluşturuldu." });
        });
    }
}