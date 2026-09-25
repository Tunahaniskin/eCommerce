using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Features.DeleteProduct;

public class DeleteProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            if (id == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçersiz ürün kimliği." });

            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            if (product.IsDeleted)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            product.MarkAsDeleted();
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Ürün başarıyla silindi." });
        })
        .RequirePermission(Permissions.Product.Delete);
    }
}
