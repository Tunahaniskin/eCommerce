using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Entities;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.UpdateRole;

public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleResult>
{
    private readonly AppDbContext _dbContext;
    private readonly UserPermissionService _permissionService;

    public UpdateRoleCommandHandler(AppDbContext dbContext, UserPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<UpdateRoleResult> HandleAsync(UpdateRoleCommand request, CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Rol bulunamadı.");

        var trimmedName = request.Name.Trim();

        // 1. SuperAdmin koruması: İsmi değiştirilemez
        var isSuperAdmin = role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
        if (isSuperAdmin && !trimmedName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("SuperAdmin rolünün ismi değiştirilemez.");
        }

        // 2. Rol adı benzersizliği kontrolü
        var nameExists = await _dbContext.Roles
            .AnyAsync(r => r.Name == trimmedName && r.Id != request.Id, cancellationToken);

        if (nameExists)
            throw new Exception("Bu isimde başka bir rol zaten mevcut.");

        // 3. Rol bilgilerini güncelle
        role.Update(trimmedName, request.Description?.Trim());

        // 4. İzinleri senkronize et
        _dbContext.RolePermissions.RemoveRange(role.Permissions);

        var newPermissions = request.Permissions?.Distinct().Where(p => !string.IsNullOrWhiteSpace(p)).ToList() 
                             ?? new List<string>();

        // SuperAdmin'in joker (*) yetkisinin silinmesi engellenir
        if (isSuperAdmin && !newPermissions.Contains(Permissions.Wildcard))
        {
            newPermissions.Add(Permissions.Wildcard);
        }

        foreach (var perm in newPermissions)
        {
            _dbContext.RolePermissions.Add(new RolePermission(role.Id, perm.Trim()));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5. Bu role sahip tüm kullanıcıların RAM önbelleğini geçersiz kıl
        var affectedUserIds = await _dbContext.Users
            .Where(u => u.RoleId == role.Id)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in affectedUserIds)
        {
            _permissionService.InvalidateUserCache(userId);
        }

        return new UpdateRoleResult("Rol ve yetkileri başarıyla güncellendi.");
    }
}
