using System;

namespace ECommerce.API.Modules.Payment.Features.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(Guid OrderId);

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string? TransactionId,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
