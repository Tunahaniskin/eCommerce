using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            LoginRequest request, 
            AppDbContext dbContext, 
            JwtProvider jwtProvider,
            HttpContext context) =>
        {
            // 1. Kullanıcıyı bul
            var user = await dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (user is null)
                return Results.BadRequest(new { Message = "Geçersiz e-posta veya şifre." });

            if (user.IsDeleted)
            {
                ECommerce.API.Infrastructure.Logging.AppLogger.Warn($"Giriş denemesi engellendi. Hesap pasif/silinmiş. Email={request.Email}");
                return Results.BadRequest(new { Message = "Hesabınız pasife alınmıştır." });
            }

            // 2. Şifreyi doğrula
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (!isPasswordValid)
                return Results.BadRequest(new { Message = "Geçersiz e-posta veya şifre." });

            // 3. Geçerliyse Token üret
            var token = jwtProvider.GenerateToken(user);

            // 4. Cache Warm-up (Önbellek Isıtma)
            // İstemci giriş yapar yapmaz ilk isteğinde DB'ye gidilmesin diye RAM'i önceden dolduruyoruz.
            var permissionService = context.RequestServices.GetRequiredService<ECommerce.API.Modules.Auth.Services.UserPermissionService>();
            await permissionService.GetUserAuthSnapshotAsync(user.Id);

            var response = new LoginResponse(
                AccessToken: token,
                ExpiresIn: 7200, // 2 saat (saniye cinsinden)
                User: new UserProfileDto(user.Id, user.Email, user.Role?.Name ?? "")
            );

            return Results.Ok(response);
        })
        .AllowAnonymous(); // Giriş yapma ekranı herkese açık olmalıdır
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, int ExpiresIn, UserProfileDto User);
public record UserProfileDto(Guid Id, string Email, string RoleName);