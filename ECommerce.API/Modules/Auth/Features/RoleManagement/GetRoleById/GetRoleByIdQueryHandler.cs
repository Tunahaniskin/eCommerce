using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetRoleById;

public class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, GetRoleByIdResult>
{
    private readonly AppDbContext _dbContext;

    public GetRoleByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetRoleByIdResult> HandleAsync(GetRoleByIdQuery request, CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Rol bulunamadı.");

        var dto = new RoleDetailsDto(
            role.Id,
            role.Name,
            role.Description,
            role.Permissions.Select(p => p.Permission).ToList()
        );

        return new GetRoleByIdResult(dto);
    }
}
