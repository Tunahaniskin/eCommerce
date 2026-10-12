using System;

namespace ECommerce.API.Modules.Finance.Entities;

public class DepositTicket
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? AdminNote { get; private set; }
    public Guid? ApprovedByAdminId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private DepositTicket() { } // EF Core

    public DepositTicket(Guid userId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        Status = TicketStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid adminId)
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("Only pending tickets can be approved.");

        Status = TicketStatus.Approved;
        ApprovedByAdminId = adminId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(Guid adminId, string reason)
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("Only pending tickets can be rejected.");

        Status = TicketStatus.Rejected;
        ApprovedByAdminId = adminId;
        AdminNote = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}
