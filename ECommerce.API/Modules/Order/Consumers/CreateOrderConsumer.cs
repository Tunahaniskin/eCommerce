using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Order.Consumers;

public class CreateOrderConsumer : IConsumer<CreateOrderMessage>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderConsumer(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CreateOrderMessage> context)
    {
        var message = context.Message;

        // Idempotency: Mesaj RabbitMQ tarafından ikinci kez gönderilirse mükerrer kaydı engelle
        if (await _dbContext.Orders.AnyAsync(o => o.Id == message.OrderId))
            return;

        var orderItems = new List<OrderItem>();

        foreach (var item in message.Items)
        {
            // Fiyatı veritabanından okuyarak güvenliği sağlıyoruz
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                orderItems.Add(new OrderItem(item.ProductId, item.Quantity, product.Price));
            }
        }

        // Eğer ürünlerin hiçbiri kalmamışsa veya silinmişse siparişi oluşturma
        if (!orderItems.Any())
            return;

        var order = new Entities.Order(message.OrderId, orderItems);
        
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Sipariş fiziksel olarak oluştu, Saga'nın bir sonraki adımı olan stok rezervasyonunu tetikle
        await _publishEndpoint.Publish(new OrderCreatedEvent(order.Id, message.Items));
    }
}