using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            [AsParameters] GetProductsRequest request, 
            HttpContext httpContext,
            IQueryHandler<GetProductsQuery, ECommerce.API.Infrastructure.Models.PagedResult<ProductResponse>> handler,
            ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService) =>
        {
            if (request.Page < 1)
                return Results.BadRequest(new { Message = "Sayfa numarası 1'den küçük olamaz." });
            if (request.PageSize < 1 || request.PageSize > 100)
                return Results.BadRequest(new { Message = "Sayfa boyutu 1 ile 100 arasında olmalıdır." });

            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;
            
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? user.FindFirst("sub")?.Value;
            
            bool canReadDeleted = false;

            if (Guid.TryParse(userIdStr, out var userId))
            {
                var authSnapshot = await permissionService.GetUserAuthSnapshotAsync(userId);
                canReadDeleted = authSnapshot?.HasPermission(Permissions.Product.ReadDeleted) ?? false;
            }

            var statusKey = request.Status?.ToLower() ?? "active";

            if ((statusKey == "deleted" || statusKey == "all") && !canReadDeleted)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Erişim Reddedildi",
                    detail: "Silinmiş ürünleri listeleme yetkiniz bulunmuyor.");
            }

            var query = new GetProductsQuery(
                request.Page, 
                request.PageSize, 
                request.Status, 
                canReadDeleted,
                request.SortBy,
                request.SortOrder,
                request.Search,
                request.CategoryId,
                request.StockStatus,
                request.MinPrice,
                request.MaxPrice
            );
            var products = await handler.HandleAsync(query);

            return Results.Ok(products);
        });
    }
}

public record GetProductsRequest(
    int Page = 1, 
    int PageSize = 20, 
    string? Status = "active",
    string? SortBy = null,
    string? SortOrder = "desc",
    string? Search = null,
    Guid? CategoryId = null,
    string? StockStatus = "all",
    decimal? MinPrice = null,
    decimal? MaxPrice = null
);

public record ProductResponse(
    Guid Id, 
    string Name, 
    decimal Price, 
    int Stock, 
    bool IsDeleted, 
    Guid CategoryId, 
    string CategoryName, 
    bool HasImage,
    string? ThumbnailPath
);
