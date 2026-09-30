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
            IQueryHandler<GetProductsQuery, List<ProductResponse>> handler,
            ECommerce.API.Modules.Auth.Services.IUserPermissionService permissionService) =>
        {
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var user = authResult.Principal ?? httpContext.User;
            
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? user.FindFirst("sub")?.Value;
            
            bool canReadDeleted = false;

            if (Guid.TryParse(userIdStr, out var userId))
            {
                var permissions = await permissionService.GetUserPermissionsAsync(userId, default);
                canReadDeleted = permissions.Contains("*") || permissions.Contains(Permissions.Product.ReadDeleted);
            }

            var statusKey = request.Status?.ToLower() ?? "active";

            if ((statusKey == "deleted" || statusKey == "all") && !canReadDeleted)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Erişim Reddedildi",
                    detail: "Silinmiş ürünleri listeleme yetkiniz bulunmuyor.");
            }

            var query = new GetProductsQuery(request.Page, request.PageSize, request.Status, canReadDeleted);
            var products = await handler.HandleAsync(query);

            return Results.Ok(products);
        });
    }
}

public record GetProductsRequest(int Page = 1, int PageSize = 10, string? Status = "active");

public record ProductResponse(Guid Id, string Name, decimal Price, int Stock, bool IsDeleted);
