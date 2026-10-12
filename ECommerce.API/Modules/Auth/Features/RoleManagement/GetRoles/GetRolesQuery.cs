namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoles;

public record GetRolesQuery();
public record RoleDto(Guid Id, string Name, string? Description, List<string> Permissions);
public record GetRolesResult(List<RoleDto> Roles);
