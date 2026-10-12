namespace ECommerce.API.Modules.Auth.Features.RoleManagement.CreateUser;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password, Guid RoleId);
public record CreateUserResult(Guid UserId, string Message);
