using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.DeleteUser;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, DeleteUserResult>
{
    private readonly AppDbContext _dbContext;
    private readonly ECommerce.API.Modules.Auth.Services.UserPermissionService _permissionService;

    public DeleteUserCommandHandler(AppDbContext dbContext, ECommerce.API.Modules.Auth.Services.UserPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<DeleteUserResult> HandleAsync(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new Exception("Kullanıcı bulunamadı."); // NotFoundException

        if (user.IsDeleted)
            throw new Exception("Kullanıcı zaten pasif/silinmiş durumda."); // BadRequest/ValidationException

        user.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Kullanıcı silindiği için RAM önbelleğini anında temizliyoruz
        _permissionService.InvalidateUserCache(user.Id);

        return new DeleteUserResult("Kullanıcı başarıyla pasife alındı.");
    }
}
