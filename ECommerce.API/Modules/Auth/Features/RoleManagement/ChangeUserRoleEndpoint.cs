using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class ChangeUserRoleEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{userId:guid}/role", async (Guid userId, ChangeUserRoleRequest request, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Results.NotFound(new { Message = "Kullanıcı bulunamadı." });

            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (role == null)
                return Results.BadRequest(new { Message = "Belirtilen rol bulunamadı." });

            user.ChangeRole(role.Id);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Kullanıcının rolü başarıyla değiştirildi." });
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Roles.Manage));
    }
}

public record ChangeUserRoleRequest(Guid RoleId);
