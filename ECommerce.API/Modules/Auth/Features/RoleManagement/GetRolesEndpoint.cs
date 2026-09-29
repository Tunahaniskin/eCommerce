using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class GetRolesEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async (AppDbContext dbContext) =>
        {
            var roles = await dbContext.Roles
                .Include(r => r.Permissions)
                .Select(r => new {
                    r.Id,
                    r.Name,
                    Permissions = r.Permissions.Select(p => p.Permission).ToList()
                })
                .ToListAsync();

            return Results.Ok(roles);
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Roles.View));
    }
}
