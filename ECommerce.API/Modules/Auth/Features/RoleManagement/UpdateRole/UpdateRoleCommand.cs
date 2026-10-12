namespace ECommerce.API.Modules.Auth.Features.RoleManagement.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string>? Permissions
);

public record UpdateRoleResult(string Message);
