using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.RestoreUser;

public class RestoreUserEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{id:guid}/restore", async (Guid id, ICommandHandler<RestoreUserCommand, RestoreUserResult> handler) =>
        {
            var result = await handler.HandleAsync(new RestoreUserCommand(id));
            return Results.Ok(result);
        })
        .RequirePermission(Permissions.Users.Manage);
    }
}
