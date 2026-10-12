namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUserById;

public record GetUserByIdQuery(Guid Id);

public record UserDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string RoleName,
    Guid RoleId,
    List<string> Permissions,
    bool IsDeleted,
    DateTime CreatedAt
);

public record GetUserByIdResult(UserDetailsDto User);
