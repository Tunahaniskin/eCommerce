using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, AppDbContext dbContext) =>
        {
            // Aynı mail ile kayıt olunmasını engelle
            if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
                return Results.BadRequest(new { Message = "Bu e-posta adresi zaten kullanılıyor." });

            var roleName = string.IsNullOrWhiteSpace(request.RoleName) ? "Customer" : request.RoleName;
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            
            if (role == null)
            {
                // Eğer Customer rolü yoksa otomatik oluştur
                if (roleName == "Customer")
                {
                    role = new Role("Customer");
                    dbContext.Roles.Add(role);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    return Results.BadRequest(new { Message = "Geçersiz rol belirtildi." });
                }
            }

            // Şifreyi Hash'le (BCrypt arkada otomatik olarak 12 round salt ekler)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Kullanıcı nesnesini oluştur
            var user = new User(request.Email, passwordHash, role.Id);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Kullanıcı başarıyla oluşturuldu.", UserId = user.Id });
        })
        .AllowAnonymous(); // Global Fallback gelse bile bu rotaya açık erişim ver
    }
}

public record RegisterRequest(string Email, string Password, string? RoleName);