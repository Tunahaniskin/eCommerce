using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Auth.Features.Profile.ChangePassword;

using Microsoft.AspNetCore.Http;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, IResult>
{
    private readonly AppDbContext _dbContext;

    public ChangePasswordCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { command.UserId }, cancellationToken);
        if (user == null)
        {
            throw new System.InvalidOperationException("Kullanıcı bulunamadı.");
        }

        bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            var warningLog = SystemLog.CreateAppLog(
                traceId: null,
                userId: command.UserId.ToString(),
                logType: "Security",
                level: "Warning",
                source: "ChangePassword",
                message: $"Admin [{command.UserId}] hatalı şifre denemesi yaptı."
            );
            _dbContext.SystemLogs.Add(warningLog);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            throw new System.InvalidOperationException("Mevcut şifreniz hatalı.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 6)
        {
            throw new System.ArgumentException("Yeni şifre en az 6 karakter olmalıdır.");
        }

        if (BCrypt.Net.BCrypt.Verify(command.NewPassword, user.PasswordHash))
        {
            throw new System.ArgumentException("Yeni şifre eski şifre ile aynı olamaz.");
        }

        var newHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword, workFactor: 11);
        user.ChangePassword(newHash);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }
}
