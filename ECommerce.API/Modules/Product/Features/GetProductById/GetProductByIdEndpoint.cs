using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce.API.Modules.Product.Features.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (
            Guid id, 
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            // Token geldiyse HttpContext üzerinden authenticate et
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;

            // IgnoreQueryFilters() → HasQueryFilter(!IsDeleted) bypass edildi.
            var product = await dbContext.Products
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            // Ürün fiziksel olarak hiç yok
            if (product is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            // YENİ PBAC KONTROLÜ: Kullanıcının silinmiş ürünleri okuma izni var mı?
            bool canReadDeleted = user.HasPermission(Permissions.Product.ReadDeleted);

            // Ürün soft-delete yapılmışsa ve istek atan kişinin yetkisi YOKSA → 404 (BOLA/IDOR Koruması)
            if (product.IsDeleted && !canReadDeleted)
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
