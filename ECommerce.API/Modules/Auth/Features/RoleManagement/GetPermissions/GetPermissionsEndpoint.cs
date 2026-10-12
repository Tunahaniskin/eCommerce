using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetPermissions;

public class GetPermissionsEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions", async (IQueryHandler<GetPermissionsQuery, GetPermissionsResult> handler) =>
        {
            var result = await handler.HandleAsync(new GetPermissionsQuery());
            return Results.Ok(result.Permissions);
        })
        .RequirePermission(Permissions.Roles.View);
    }
}
