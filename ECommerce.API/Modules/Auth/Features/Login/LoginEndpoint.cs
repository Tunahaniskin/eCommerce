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
            JwtProvider jwtProvider) =>
        {
            // 1. Kullanıcıyı bul
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (user is null)
                return Results.BadRequest(new { Message = "Geçersiz e-posta veya şifre." });

            // 2. Şifreyi doğrula
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (!isPasswordValid)
                return Results.BadRequest(new { Message = "Geçersiz e-posta veya şifre." });

            // 3. Geçerliyse Token üret
            var token = jwtProvider.GenerateToken(user);

            return Results.Ok(new { AccessToken = token });
        })
        .AllowAnonymous(); // Giriş yapma ekranı herkese açık olmalıdır
    }
}

public record LoginRequest(string Email, string Password);