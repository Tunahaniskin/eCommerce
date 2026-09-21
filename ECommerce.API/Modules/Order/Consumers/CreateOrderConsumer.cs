using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Order.Consumers;

public class CreateOrderConsumer : IConsumer<CreateOrderMessage>
{
    private readonly AppDbContext _dbContext;

    // IPublishEndpoint bağımlılığı kaldırıldı
    public CreateOrderConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CreateOrderMessage> context)
    {
        var message = context.Message;

        if (await _dbContext.Orders.AnyAsync(o => o.Id == message.OrderId))
            return;

        var orderItems = new List<OrderItem>();

        foreach (var item in message.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                orderItems.Add(new OrderItem(item.ProductId, item.Quantity, product.Price));
            }
        }

        if (!orderItems.Any())
            return;

        var order = new Entities.Order(message.OrderId, orderItems);
        
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // message.Items (CreateOrderItemMessage) -> OrderItemDto listesine dönüştürülüyor
        var catalogItems = message.Items
            .Select(i => new OrderItemDto(i.ProductId, i.Quantity))
            .ToList();

        // context üzerinden fırlatarak CorrelationId zincirini koruyoruz
        await context.Publish(new OrderCreatedEvent(order.Id, catalogItems));
    }
}