using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.AssignTemporaryPermission;

public class AssignTemporaryPermissionCommandHandler : ICommandHandler<AssignTemporaryPermissionCommand, AssignTemporaryPermissionResult>
{
    private readonly AppDbContext _dbContext;
    private readonly ECommerce.API.Modules.Auth.Services.UserPermissionService _permissionService;

    public AssignTemporaryPermissionCommandHandler(AppDbContext dbContext, ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<AssignTemporaryPermissionResult> HandleAsync(AssignTemporaryPermissionCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.Include(u => u.TemporaryPermissions).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new Exception("Kullanıcı bulunamadı.");

        user.GrantTemporaryPermission(request.Permission, TimeSpan.FromHours(request.DurationInHours), request.AdminId);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Kullanıcının yetkileri değiştiği için RAM önbelleğini anında temizliyoruz.
        _permissionService.InvalidateUserCache(user.Id);

        return new AssignTemporaryPermissionResult($"Kullanıcıya geçici olarak '{request.Permission}' yetkisi verildi.");
    }
}
