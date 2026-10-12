namespace ECommerce.API.Modules.Auth.Features.RoleManagement.RestoreUser;

public record RestoreUserCommand(Guid UserId);
public record RestoreUserResult(string Message);
