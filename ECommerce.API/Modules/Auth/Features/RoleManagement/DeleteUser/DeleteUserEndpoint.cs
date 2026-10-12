using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.DeleteUser;

public class DeleteUserEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{userId:guid}", async (Guid userId, ICommandHandler<DeleteUserCommand, DeleteUserResult> handler) =>
        {
            var command = new DeleteUserCommand(userId);
            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = result.Message });
        })
        .RequirePermission(Permissions.Users.Delete);
    }
}
