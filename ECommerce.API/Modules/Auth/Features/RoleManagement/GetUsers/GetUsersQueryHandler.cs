using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUsers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    private readonly AppDbContext _dbContext;

    public GetUsersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUsersResult> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsQueryable();

        if (request.IncludeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        var users = await query
            .Include(u => u.Role)
            .Include(u => u.TemporaryPermissions)
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.CreatedAt,
                u.IsDeleted,
                new UserRoleDto(u.Role.Id, u.Role.Name),
                u.TemporaryPermissions
                    .Where(tp => tp.ExpiresAt == null || tp.ExpiresAt > DateTime.UtcNow)
                    .Select(tp => new TemporaryPermissionDto(tp.Permission, tp.ExpiresAt))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return new GetUsersResult(users);
    }
}
