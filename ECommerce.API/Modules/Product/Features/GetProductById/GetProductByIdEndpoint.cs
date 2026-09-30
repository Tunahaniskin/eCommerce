using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ECommerce.API.Modules.Product.Features.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (
            Guid id, 
            HttpContext httpContext,
            IQueryHandler<GetProductByIdQuery, ProductDetailResponse?> handler,
            ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService) =>
        {
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

            var query = new GetProductByIdQuery(id, canReadDeleted);
            var response = await handler.HandleAsync(query);

            if (response is null)
                return Results.NotFound(new { Message = "Ürün bulunamadı." });

            return Results.Ok(response);
        });
    }
}

public record ProductDetailResponse(Guid Id, string Name, decimal Price, int Stock, bool IsDeleted);
