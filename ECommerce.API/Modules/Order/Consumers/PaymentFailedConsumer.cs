using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Order.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(AppDbContext dbContext, ILogger<PaymentFailedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;
        _logger.LogWarning("[ORDER SAGA] Sipariş {OrderId} ödeme hatası aldı. Sipariş iptal ediliyor. Sebep: {Reason}", message.OrderId, message.Reason);

        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);

        if (order != null && order.Status == OrderStatus.Pending)
        {
            order.UpdateStatus(OrderStatus.Cancelled);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("[ORDER SAGA] Sipariş {OrderId} statüsü 'Cancelled' olarak güncellendi.", message.OrderId);

    }
}