using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.DeleteRole;

public class DeleteRoleEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/roles/{id:guid}", async (Guid id, ICommandHandler<DeleteRoleCommand, DeleteRoleResult> handler) =>
        {
            var result = await handler.HandleAsync(new DeleteRoleCommand(id));
            return Results.Ok(result);
        })
        .RequirePermission(Permissions.Roles.Manage);
    }
}
