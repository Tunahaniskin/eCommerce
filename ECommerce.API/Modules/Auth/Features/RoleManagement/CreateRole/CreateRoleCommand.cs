namespace ECommerce.API.Modules.Auth.Features.RoleManagement.CreateRole;

public record CreateRoleCommand(string Name, string? Description, List<string>? Permissions);
public record CreateRoleResult(Guid RoleId, string Message);
