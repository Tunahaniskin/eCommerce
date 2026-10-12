using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResult>
{
    private readonly AppDbContext _dbContext;

    public GetUserByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUserByIdResult> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .Include(u => u.TemporaryPermissions)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            throw new Exception("Kullanıcı bulunamadı.");

        // Rolün izinleri + süresi geçmemiş geçici izinler
        var permissions = new HashSet<string>();

        if (user.Role?.Permissions != null)
        {
            foreach (var rp in user.Role.Permissions)
            {
                permissions.Add(rp.Permission);
            }
        }

        var now = DateTime.UtcNow;
        if (user.TemporaryPermissions != null)
        {
            foreach (var tp in user.TemporaryPermissions.Where(p => p.ExpiresAt == null || p.ExpiresAt > now))
            {
                permissions.Add(tp.Permission);
            }
        }

        var dto = new UserDetailsDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role?.Name ?? "",
            user.RoleId,
            permissions.ToList(),
            user.IsDeleted,
            user.CreatedAt
        );

        return new GetUserByIdResult(dto);
    }
}
