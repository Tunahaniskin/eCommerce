using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;

namespace ECommerce.API.Modules.Catalog.Features.DeleteProduct;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/catalog/products/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NotFound(new { Message = "Silinmek istenen ürün bulunamadı." });
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Ürün başarıyla silindi." });
        });
    }
}