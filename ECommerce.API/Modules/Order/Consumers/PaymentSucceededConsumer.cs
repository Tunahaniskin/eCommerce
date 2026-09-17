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
        if (order is null) return;

        // Sipariş durumunu Paid (Ödendi) yapıyoruz
        order.UpdateStatus(OrderStatus.Paid);

        await _dbContext.SaveChangesAsync();
    }
}