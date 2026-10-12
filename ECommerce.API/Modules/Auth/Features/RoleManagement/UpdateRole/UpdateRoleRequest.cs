namespace ECommerce.API.Modules.Auth.Features.RoleManagement.UpdateRole;

public record UpdateRoleRequest(
    string Name,
    string? Description,
    List<string>? Permissions
);
