namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoleById;

public record GetRoleByIdQuery(Guid Id);

public record RoleDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions
);

public record GetRoleByIdResult(RoleDetailsDto Role);
