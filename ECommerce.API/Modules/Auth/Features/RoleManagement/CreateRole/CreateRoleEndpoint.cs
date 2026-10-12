using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.CreateRole;

public class CreateRoleEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles", async (CreateRoleRequest request, ICommandHandler<CreateRoleCommand, CreateRoleResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { Message = "Rol adı boş olamaz." });
            if (request.Name.Length < 2 || request.Name.Length > 50)
                return Results.BadRequest(new { Message = "Rol adı 2 ile 50 karakter arasında olmalıdır." });

            var command = new CreateRoleCommand(request.Name, request.Description, request.Permissions);
            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = result.Message, RoleId = result.RoleId });
        })
        .RequirePermission(Permissions.Roles.Manage);
    }
}

public record CreateRoleRequest(string Name, string? Description, List<string>? Permissions);
