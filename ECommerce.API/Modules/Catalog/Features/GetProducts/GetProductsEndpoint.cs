using System.Security.Claims;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/products", async (
            [AsParameters] GetProductsRequest request, 
            HttpContext httpContext,
            AppDbContext dbContext) =>
        {
            // Token geldiyse HttpContext üzerinden güvenli bir şekilde authenticate et
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;

            // Debug için Konsola Yazdır:
            Console.WriteLine($"--> Authenticated mı?: {user.Identity?.IsAuthenticated}");
            foreach (var c in user.Claims)
            {
                Console.WriteLine($"--> Claim: {c.Type} = {c.Value}");
            }

            // Role kontrolü (Hem standart URI hem düz 'role' kontrolü)
            bool isAdmin = user.IsInRole("Admin") || 
                           user.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Admin");

            Console.WriteLine($"--> isAdmin Kararı: {isAdmin}");

            // DbContext'te HasQueryFilter(p => !p.IsDeleted) tanımlı.
            // Admin "deleted" veya "all" istiyorsa bu global filtreyi IgnoreQueryFilters() ile bypass et.
            var statusKey = request.Status?.ToLower() ?? "active";

            IQueryable<ECommerce.API.Modules.Catalog.Entities.Product> query;

            if (isAdmin && (statusKey == "deleted" || statusKey == "all"))
            {
                // Global HasQueryFilter devre dışı bırakılıyor
                var rawQuery = dbContext.Products.AsNoTracking().IgnoreQueryFilters();
                query = statusKey == "deleted"
                    ? rawQuery.Where(p => p.IsDeleted)   // sadece silinmişler
                    : rawQuery;                           // "all" → hiçbir filtre yok
            }
            else
            {
                // Global Filter aktif → sadece !IsDeleted olanlar gelir (müşteri veya admin "active")
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