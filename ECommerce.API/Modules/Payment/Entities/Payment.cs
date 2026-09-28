using System;

namespace ECommerce.API.Modules.Payment.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Payment() { Currency = null!; } // EF Core için

    public Payment(Guid orderId, decimal amount, string currency = "TL")
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Yalnızca bekleyen ödemeler onaylanabilir.");

        Status = PaymentStatus.Success;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Yalnızca bekleyen ödemeler reddedilebilir.");

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        CompletedAt = DateTime.UtcNow;
    }
}
