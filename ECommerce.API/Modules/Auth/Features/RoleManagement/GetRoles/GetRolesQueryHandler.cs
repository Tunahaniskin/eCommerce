using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoles;

public class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, GetRolesResult>
{
    private readonly AppDbContext _dbContext;

    public GetRolesQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetRolesResult> HandleAsync(GetRolesQuery request, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.Permissions)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.Permissions.Select(p => p.Permission).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new GetRolesResult(roles);
    }
}
