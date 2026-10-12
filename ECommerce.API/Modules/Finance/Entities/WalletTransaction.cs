using System;

namespace ECommerce.API.Modules.Finance.Entities;

public class WalletTransaction
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public WalletTransactionType Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private WalletTransaction() { } // EF Core

    public WalletTransaction(Guid walletId, decimal amount, WalletTransactionType type, string? description = null)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        Amount = amount;
        Type = type;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}
