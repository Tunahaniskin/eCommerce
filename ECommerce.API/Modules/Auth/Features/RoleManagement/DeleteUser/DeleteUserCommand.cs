namespace ECommerce.API.Modules.Auth.Features.RoleManagement.DeleteUser;

public record DeleteUserCommand(Guid UserId);
public record DeleteUserResult(string Message);
