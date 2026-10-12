using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoleById;

public class GetRoleByIdEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles/{id:guid}", async (Guid id, IQueryHandler<GetRoleByIdQuery, GetRoleByIdResult> handler) =>
        {
            var result = await handler.HandleAsync(new GetRoleByIdQuery(id));
            return Results.Ok(result.Role);
        })
        .RequirePermission(Permissions.Roles.View);
    }
}
