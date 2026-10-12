using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Logging;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.RestoreUser;

public class RestoreUserCommandHandler : ICommandHandler<RestoreUserCommand, RestoreUserResult>
{
    private readonly AppDbContext _dbContext;
    private readonly UserPermissionService _permissionService;

    public RestoreUserCommandHandler(AppDbContext dbContext, UserPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<RestoreUserResult> HandleAsync(RestoreUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new Exception("Kullanıcı bulunamadı.");

        if (!user.IsDeleted)
            throw new Exception("Kullanıcı zaten aktif durumda.");

        user.Restore();
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Kullanıcı aktif edildiği için RAM önbelleğini temizliyoruz
        _permissionService.InvalidateUserCache(user.Id);

        // Audit Log
        AppLogger.Info($"[RESTORE USER] Kullanıcı başarıyla geri yüklendi ve aktif edildi: {user.Email} (ID: {user.Id})", source: "RestoreUser");

        return new RestoreUserResult("Kullanıcı başarıyla geri yüklendi.");
    }
}
