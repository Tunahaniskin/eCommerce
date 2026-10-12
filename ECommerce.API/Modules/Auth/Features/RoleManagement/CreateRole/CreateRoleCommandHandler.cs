using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.CreateRole;

public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResult>
{
    private readonly AppDbContext _dbContext;

    public CreateRoleCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateRoleResult> HandleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
            throw new Exception("Bu rol zaten mevcut.");

        var role = new Role(request.Name, request.Description);

        if (request.Permissions != null && request.Permissions.Any())
        {
            foreach (var perm in request.Permissions)
            {
                role.AddPermission(perm);
            }
        }

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateRoleResult(role.Id, "Rol başarıyla oluşturuldu.");
    }
}
