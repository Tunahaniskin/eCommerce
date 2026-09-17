using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;

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
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        
        if (order != null && order.Status == OrderStatus.Pending)
        {
            order.UpdateStatus(OrderStatus.Cancelled);
            await _dbContext.SaveChangesAsync();
        }
    }
}