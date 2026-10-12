using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.UpdateRole;

public class UpdateRoleEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/roles/{id:guid}", async (
            Guid id, 
            UpdateRoleRequest request, 
            ICommandHandler<UpdateRoleCommand, UpdateRoleResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { Message = "Rol adı boş olamaz." });
            if (request.Name.Length < 2 || request.Name.Length > 50)
                return Results.BadRequest(new { Message = "Rol adı 2 ile 50 karakter arasında olmalıdır." });

            var command = new UpdateRoleCommand(id, request.Name, request.Description, request.Permissions);
            var result = await handler.HandleAsync(command);

            return Results.Ok(result);
        })
        .RequirePermission(Permissions.Roles.Manage);
    }
}
