using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class CreateUserEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (CreateAdminUserRequest request, AppDbContext dbContext) =>
        {
            if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
                return Results.BadRequest(new { Message = "Bu e-posta adresi zaten kullanılıyor." });

            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (role == null)
                return Results.BadRequest(new { Message = "Geçersiz rol." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User(request.Email, passwordHash, role.Id);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Kullanıcı başarıyla oluşturuldu.", UserId = user.Id });
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Users.Manage));
    }
}

public record CreateAdminUserRequest(string Email, string Password, Guid RoleId);
