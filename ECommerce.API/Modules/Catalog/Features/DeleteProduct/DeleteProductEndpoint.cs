using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.DeleteProduct;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Route constraint eklendi: "{id:guid}"
        app.MapDelete("/api/catalog/products/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            if (id == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçersiz ürün kimliği." });

            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            // Zaten silinmişse tekrar silmeye çalışma (Idempotency / Hata önleme)
            if (product.IsDeleted)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            product.MarkAsDeleted();
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Ürün başarıyla silindi." });
        });
    }
}