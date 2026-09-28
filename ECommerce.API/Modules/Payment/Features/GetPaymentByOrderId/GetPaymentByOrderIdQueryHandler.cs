using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Payment.Features.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IQueryHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    private readonly AppDbContext _dbContext;

    public GetPaymentByOrderIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentDto?> HandleAsync(GetPaymentByOrderIdQuery query, CancellationToken cancellationToken = default)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == query.OrderId, cancellationToken);

        if (payment is null)
            return null;

        return new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.TransactionId,
            payment.FailureReason,
            payment.CreatedAt,
            payment.CompletedAt
        );
    }
}
