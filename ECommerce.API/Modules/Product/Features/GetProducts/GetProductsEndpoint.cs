using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            [AsParameters] GetProductsRequest request, 
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            // Token geldiyse HttpContext üzerinden güvenli bir şekilde authenticate et
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;

            // YENİ PBAC KONTROLÜ: Kullanıcının silinmiş ürünleri okuma izni var mı?
            bool canReadDeleted = user.HasPermission(Permissions.Product.ReadDeleted);

            var statusKey = request.Status?.ToLower() ?? "active";

            // YETKİ KONTROLÜ: Kullanıcı silinmiş ürünleri ("deleted" veya "all") istiyorsa ama yetkisi yoksa kapıdan çevir
            if ((statusKey == "deleted" || statusKey == "all") && !canReadDeleted)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Erişim Reddedildi",
                    detail: "Silinmiş ürünleri listeleme yetkiniz bulunmuyor.");
            }

            IQueryable<ECommerce.API.Modules.Product.Entities.Product> query;

            // YETKİ ONAYLANDI: Kullanıcının yetkisi var ve silinmiş/tüm ürünleri görmek istiyor
            if (canReadDeleted && (statusKey == "deleted" || statusKey == "all"))
            {
                // Global HasQueryFilter (IsDeleted == false) devre dışı bırakılıyor
                var rawQuery = dbContext.Products.AsNoTracking().IgnoreQueryFilters();
                query = statusKey == "deleted"
                    ? rawQuery.Where(p => p.IsDeleted)   // Sadece silinmişler
                    : rawQuery;                          // "all" → Filtre yok, hepsi
            }
            else
            {
                // Standart müşteri veya "active" ürün isteyen yetkili
                query = dbContext.Products.AsNoTracking();
            }

            query = query.OrderByDescending(p => p.Id); 

            var products = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductResponse(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.IsDeleted
                ))
                .ToListAsync();

            return Results.Ok(products);
        });
    }
}

public record GetProductsRequest(int Page = 1, int PageSize = 10, string? Status = "active");

public record ProductResponse(Guid Id, string Name, decimal Price, int Stock, bool IsDeleted);
