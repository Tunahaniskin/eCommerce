using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Finance.Entities;
using ECommerce.API.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Finance.Features.AdjustWalletBalance;

public class AdjustWalletBalanceCommandHandler : ICommandHandler<AdjustWalletBalanceCommand, IResult>
{
    private readonly AppDbContext _dbContext;

    public AdjustWalletBalanceCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IResult> HandleAsync(AdjustWalletBalanceCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            throw new ArgumentException("Tutar sıfırdan büyük olmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            throw new ArgumentException("Açıklama (sebep) zorunludur.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var wallet = await _dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == command.UserId, cancellationToken);

            if (wallet == null)
            {
                if (command.ActionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
                {
                    wallet = new Wallet(command.UserId, "TL");
                    await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException("Kullanıcıya ait cüzdan bulunamadı. Önce bakiye yüklemesi yapılmalıdır.");
                }
            }

            decimal amountChange = 0;


            if (command.ActionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
            {
                wallet.AddBalance(command.Amount);
                amountChange = command.Amount;
            }
            else if (command.ActionType.Equals("Deduct", StringComparison.OrdinalIgnoreCase))
            {
                if (wallet.Balance < command.Amount)
                {
                    throw new InvalidOperationException($"Yetersiz bakiye. Kullanıcının mevcut bakiyesi: {wallet.Balance:C}");
                }
                wallet.DeductBalance(command.Amount);
                amountChange = -command.Amount;
            }
            else
            {
                throw new ArgumentException("Geçersiz işlem tipi. Sadece 'Deposit' veya 'Deduct' olabilir.");
            }

            var walletTransaction = new WalletTransaction(
                wallet.Id,
                amountChange,
                WalletTransactionType.AdminAdjustment,
                $"[Admin Müdahalesi] {command.Reason}"
            );

            await _dbContext.WalletTransactions.AddAsync(walletTransaction, cancellationToken);

            var auditLog = SystemLog.CreateAppLog(
                traceId: null,
                userId: command.AdminUserId.ToString(),
                logType: "Audit",
                level: "Information",
                source: "AdjustWalletBalance",
                message: $"Admin [{command.AdminUserId}], Kullanıcı [{command.UserId}] cüzdanında bakiye güncellemesi yaptı. İşlem: {command.ActionType}, Tutar: {command.Amount}, Yeni Bakiye: {wallet.Balance}. Sebep: {command.Reason}"
            );

            _dbContext.SystemLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Microsoft.AspNetCore.Http.Results.Ok(new { message = "Bakiye başarıyla güncellendi.", newBalance = wallet.Balance });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
