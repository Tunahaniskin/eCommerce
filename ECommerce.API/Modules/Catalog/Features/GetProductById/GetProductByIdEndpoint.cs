using System.Security.Claims;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/products/{id:guid}", async (
            Guid id, 
            ClaimsPrincipal user, 
            AppDbContext dbContext) =>
        {
            // IgnoreQueryFilters() → HasQueryFilter(!IsDeleted) bypass edildi.
            // Böylece silinmiş ürünü de veritabanından çekebiliyoruz;
            // ardından kim istediğine göre karar veriyoruz.
            var product = await dbContext.Products
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            // Ürün fiziksel olarak hiç yok
            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            // Role kontrolü (GetProductsEndpoint ile aynı yöntem)
            bool isAdmin = user.IsInRole("Admin") ||
                           user.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Admin");

            // Ürün soft-delete yapılmışsa ve istek atan Admin DEĞİLSE → 404 (BOLA Koruması)
            if (product.IsDeleted && !isAdmin)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            var response = new ProductDetailResponse(
                product.Id,
                product.Name,
                product.Price,
                product.Stock,
                product.IsDeleted
            );

            return Results.Ok(response);
        });
    }
}

public record ProductDetailResponse(Guid Id, string Name, decimal Price, int Stock, bool IsDeleted);