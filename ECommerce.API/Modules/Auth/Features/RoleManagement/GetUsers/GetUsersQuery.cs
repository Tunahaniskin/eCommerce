namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUsers;

public record GetUsersQuery(bool IncludeDeleted = false);

public record UserRoleDto(Guid Id, string Name);
public record TemporaryPermissionDto(string Permission, DateTime? ExpiresAt);

public record UserDto(
    Guid Id, 
    string FirstName, 
    string LastName, 
    string Email, 
    DateTime CreatedAt, 
    bool IsDeleted, 
    UserRoleDto Role, 
    List<TemporaryPermissionDto> TemporaryPermissions);

public record GetUsersResult(List<UserDto> Users);
