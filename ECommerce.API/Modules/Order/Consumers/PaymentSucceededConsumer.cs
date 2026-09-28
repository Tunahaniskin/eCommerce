using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Order.Consumers;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly AppDbContext _dbContext;

    public PaymentSucceededConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;

        var order = await _dbContext.Orders.FindAsync(message.OrderId);
        
        // Idempotency: Sipariş Pending veya StockReserved durumundaysa Paid yapılmalı.
        if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.StockReserved))
        {
            order.MarkAsPaid();
            await _dbContext.SaveChangesAsync();
        }
    }
}