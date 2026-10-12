using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Auth.Features.Profile.GetProfile;

public class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, ProfileResponseDto?>
{
    private readonly AppDbContext _dbContext;

    public GetProfileQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfileResponseDto?> HandleAsync(GetProfileQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user == null)
            return null;

        return new ProfileResponseDto(
            user.FirstName,
            user.LastName,
            user.Email
        );
    }
}
