using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Security;
using ECommerce.API.Modules.Auth.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement;

public class DeleteUserEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{userId:guid}", async (Guid userId, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Results.NotFound(new { Message = "Kullanıcı bulunamadı." });

            if (user.IsDeleted)
                return Results.BadRequest(new { Message = "Kullanıcı zaten pasif/silinmiş durumda." });

            user.MarkAsDeleted();
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { Message = "Kullanıcı başarıyla pasife alındı." });
        })
        .AddEndpointFilter(new PermissionEndpointFilter(Permissions.Users.Delete));
    }
}
