using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class GetUsersEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (AppDbContext dbContext) =>
        {
            var users = await dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.TemporaryPermissions)
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.CreatedAt,
                    u.IsDeleted,
                    Role = new { u.Role.Id, u.Role.Name },
                    TemporaryPermissions = u.TemporaryPermissions
                        .Where(tp => tp.ExpiresAt == null || tp.ExpiresAt > DateTime.UtcNow)
                        .Select(tp => new { tp.Permission, tp.ExpiresAt })
                        .ToList()
                })
                .ToListAsync();

            return Results.Ok(users);
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Users.View));
    }
}
