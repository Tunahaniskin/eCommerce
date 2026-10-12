using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.ChangeUserRole;

public class ChangeUserRoleCommandHandler : ICommandHandler<ChangeUserRoleCommand, ChangeUserRoleResult>
{
    private readonly AppDbContext _dbContext;
    private readonly ECommerce.API.Modules.Auth.Services.UserPermissionService _permissionService;

    public ChangeUserRoleCommandHandler(AppDbContext dbContext, ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<ChangeUserRoleResult> HandleAsync(ChangeUserRoleCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new Exception("Kullanıcı bulunamadı."); // Ideal: NotFoundException

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
            throw new Exception("Belirtilen rol bulunamadı."); // Ideal: ValidationException

        user.ChangeRole(role.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Kullanıcının yetkileri değiştiği için RAM önbelleğini anında temizliyoruz.
        _permissionService.InvalidateUserCache(user.Id);

        return new ChangeUserRoleResult("Kullanıcının rolü başarıyla değiştirildi.");
    }
}
