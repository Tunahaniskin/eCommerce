using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using ECommerce.API.Infrastructure.Logging;

namespace ECommerce.API.Modules.Order.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly AppDbContext _dbContext;
    public PaymentFailedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;
        AppLogger.Warn($"[ORDER SAGA] Sipariş {message.OrderId} ödeme hatası aldı. Sipariş iptal ediliyor. Sebep: {message.Reason}");

        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);

        if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.StockReserved))
        {
            order.Cancel(message.Reason);
            await _dbContext.SaveChangesAsync();
        }

        AppLogger.Info($"[ORDER SAGA] Sipariş {message.OrderId} statüsü 'Cancelled' olarak güncellendi.");

    }
}