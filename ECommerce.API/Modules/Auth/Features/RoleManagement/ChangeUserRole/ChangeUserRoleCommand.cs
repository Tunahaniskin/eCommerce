namespace ECommerce.API.Modules.Auth.Features.RoleManagement.ChangeUserRole;

public record ChangeUserRoleCommand(Guid UserId, Guid RoleId);
public record ChangeUserRoleResult(string Message);
