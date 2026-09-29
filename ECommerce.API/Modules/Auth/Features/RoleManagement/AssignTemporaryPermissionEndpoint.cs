using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class AssignTemporaryPermissionEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:guid}/permissions/temporary", async (Guid userId, AssignTemporaryPermissionRequest request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.Include(u => u.TemporaryPermissions).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Results.NotFound(new { Message = "Kullanıcı bulunamadı." });

            var adminIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out Guid adminId))
                return Results.Unauthorized();

            user.GrantTemporaryPermission(request.Permission, TimeSpan.FromHours(request.DurationInHours), adminId);
            
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = $"Kullanıcıya geçici olarak '{request.Permission}' yetkisi verildi." });
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Roles.Manage));
    }
}

public record AssignTemporaryPermissionRequest(string Permission, int DurationInHours);
