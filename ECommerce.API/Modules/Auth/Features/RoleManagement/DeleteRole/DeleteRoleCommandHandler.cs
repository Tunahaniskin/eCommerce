using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.DeleteRole;

public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand, DeleteRoleResult>
{
    private readonly AppDbContext _dbContext;

    public DeleteRoleCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteRoleResult> HandleAsync(DeleteRoleCommand request, CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Rol bulunamadı.");

        // 1. SuperAdmin ve varsayılan sistem rolleri silinemez
        if (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("SuperAdmin veya varsayılan sistem rolleri silinemez.");
        }

        // 2. Bu role sahip kullanıcı var mı kontrolü
        var hasUsers = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.RoleId == request.Id, cancellationToken);

        if (hasUsers)
        {
            throw new Exception("Bu role atanmış kullanıcılar bulunduğu için rol silinemez. Lütfen önce kullanıcıların rolünü değiştirin.");
        }

        // 3. İzinleri ve rolü kaldır
        _dbContext.RolePermissions.RemoveRange(role.Permissions);
        _dbContext.Roles.Remove(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteRoleResult("Rol başarıyla silindi.");
    }
}
