namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetPermissions;

public record GetPermissionsQuery();
public record GetPermissionsResult(List<string> Permissions);
