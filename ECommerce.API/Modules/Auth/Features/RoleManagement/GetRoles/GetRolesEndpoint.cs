using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoles;

public class GetRolesEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async (IQueryHandler<GetRolesQuery, GetRolesResult> handler) =>
        {
            var result = await handler.HandleAsync(new GetRolesQuery());
            return Results.Ok(result.Roles);
        })
        .RequirePermission(Permissions.Roles.View);
    }
}
