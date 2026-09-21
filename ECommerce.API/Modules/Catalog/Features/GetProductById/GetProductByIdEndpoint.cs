using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Route constraint eklendi: "{id:guid}"
        app.MapGet("/api/catalog/products/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            // Veritabanına gitmeden önce boş Guid kontrolü (Fail-Fast)
            if (id == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçersiz ürün kimliği." });

            // AsNoTracking ile performansı artır ve IsDeleted olanları filtrele
            var product = await dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new 
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock
                })
                .FirstOrDefaultAsync();

            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            return Results.Ok(product);
        });
    }
}