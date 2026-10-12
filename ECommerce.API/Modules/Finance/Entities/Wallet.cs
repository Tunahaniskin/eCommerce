using System;

namespace ECommerce.API.Modules.Finance.Entities;

public class Wallet
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Wallet() { } // EF Core

    public Wallet(Guid userId, string currency = "TL")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Balance = 0;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBalance(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductBalance(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance.");

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }
}
