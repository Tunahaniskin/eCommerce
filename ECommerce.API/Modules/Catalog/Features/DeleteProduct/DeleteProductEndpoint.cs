using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Catalog.Features.DeleteProduct;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/catalog/products/{id:guid}", async (
            Guid id, 
            AppDbContext dbContext,
            IPublishEndpoint publishEndpoint) =>
        {
            // FindAsync, Global Query Filter'ı ezer mi? 
            // FindAsync doğrudan primary key ile arar, filter devrededir.
            var product = await dbContext.Products.FindAsync(id);

            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı veya zaten silinmiş." });

            // Domain metodunu tetikle
            product.MarkAsDeleted();
            
            await dbContext.SaveChangesAsync();

            // (Opsiyonel) Sistemdeki diğer modülleri haberdar et (Örn: Basket modülünde sepetteyse silinsin)
            await publishEndpoint.Publish(new ProductDeletedEvent(product.Id));

            // Silme işlemlerinde REST standardı genellikle 204 No Content'tir.
            return Results.NoContent(); 
        });
    }
}

// Contracts klasörüne (Shared/Contracts/OrderEvents.cs) eklenecek sözleşme:
// public record ProductDeletedEvent(Guid ProductId);