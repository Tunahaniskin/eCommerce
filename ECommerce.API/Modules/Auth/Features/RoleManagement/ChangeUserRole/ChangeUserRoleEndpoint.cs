using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.ChangeUserRole;

public class ChangeUserRoleEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{userId:guid}/role", async (Guid userId, ChangeUserRoleRequest request, ICommandHandler<ChangeUserRoleCommand, ChangeUserRoleResult> handler) =>
        {
            if (request.RoleId == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçerli bir Rol ID belirtilmelidir." });

            var command = new ChangeUserRoleCommand(userId, request.RoleId);
            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = result.Message });
        })
        .RequirePermission(Permissions.Roles.Manage);
    }
}

public record ChangeUserRoleRequest(Guid RoleId);
