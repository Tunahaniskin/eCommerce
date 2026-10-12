namespace ECommerce.API.Modules.Auth.Features.RoleManagement.AssignTemporaryPermission;

public record AssignTemporaryPermissionCommand(Guid UserId, string Permission, int DurationInHours, Guid AdminId);
public record AssignTemporaryPermissionResult(string Message);
