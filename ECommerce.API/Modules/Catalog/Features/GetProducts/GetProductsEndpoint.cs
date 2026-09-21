using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Validation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.GetProducts;

// Minimal API'de query string parametrelerini bu record'a bağlayacağız
public record GetProductsRequest(int Page = 1, int PageSize = 10);

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // [AsParameters] attribute'u URL'deki ?Page=x&PageSize=y parametrelerini record'a doldurur
        app.MapGet("/api/catalog/products", async ([AsParameters] GetProductsRequest request, AppDbContext dbContext) =>
        {
            // 1. Temel Sorgu: İzlemeyi kapat ve silinenleri gizle
            var query = dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            // 2. Toplam Kayıt Sayısı (Sayfalama hesabı için gerekli)
            var totalCount = await query.CountAsync();

            // 3. Veriyi Çekme (Skip ve Take ile)
            var products = await query
                .OrderBy(p => p.Id) // Sayfalamanın tutarlı çalışması için mutlaka bir sıralama (Order) olmalıdır
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new 
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock
                })
                .ToListAsync();

            // 4. İstemciye standart sayfalama nesnesi dön
            return Results.Ok(new
            {
                Data = products,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            });
        })
        .AddEndpointFilter<ValidationFilter<GetProductsRequest>>(); // Sayfalama validasyonu devrede
    }
}