using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.CreateUser;

public class CreateUserEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (CreateAdminUserRequest request, ICommandHandler<CreateUserCommand, CreateUserResult> handler) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
                return Results.BadRequest(new { Message = "Ad alanı boş olamaz." });
            if (string.IsNullOrWhiteSpace(request.LastName))
                return Results.BadRequest(new { Message = "Soyad alanı boş olamaz." });
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest(new { Message = "E-posta boş olamaz." });
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return Results.BadRequest(new { Message = "Şifre en az 6 karakter olmalıdır." });
            if (request.RoleId == Guid.Empty)
                return Results.BadRequest(new { Message = "Geçerli bir Rol ID belirtilmelidir." });

            var command = new CreateUserCommand(request.FirstName, request.LastName, request.Email, request.Password, request.RoleId);
            var result = await handler.HandleAsync(command);

            return Results.Ok(new { Message = result.Message, UserId = result.UserId });
        })
        .RequirePermission(Permissions.Users.Manage);
    }
}

public record CreateAdminUserRequest(string FirstName, string LastName, string Email, string Password, Guid RoleId);
