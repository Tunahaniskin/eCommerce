using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Payment.Providers;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, string currency, CancellationToken cancellationToken = default);
}

public record PaymentResult(bool IsSuccess, string? TransactionId, string? ErrorMessage);
