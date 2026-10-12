using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Auth.Features.Profile.UpdateProfile;

using Microsoft.AspNetCore.Http;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, IResult>
{
    private readonly AppDbContext _dbContext;

    public UpdateProfileCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName) || string.IsNullOrWhiteSpace(command.LastName))
        {
            throw new System.ArgumentException("İsim ve soyisim boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !new EmailAddressAttribute().IsValid(command.Email))
        {
            throw new System.ArgumentException("Geçerli bir e-posta adresi giriniz.");
        }

        var isEmailInUse = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == command.Email.ToLower() && u.Id != command.UserId, cancellationToken);

        if (isEmailInUse)
        {
            throw new System.InvalidOperationException("Bu e-posta adresi kullanımda.");
        }

        var user = await _dbContext.Users.FindAsync(new object[] { command.UserId }, cancellationToken);
        if (user == null)
        {
            throw new System.InvalidOperationException("Kullanıcı bulunamadı.");
        }

        user.UpdateName(command.FirstName, command.LastName);
        user.UpdateEmail(command.Email);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }
}
