using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AppDbContext _dbContext;

    public OrderCreatedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        bool reservationFailed = false;
        string failureReason = string.Empty;
        decimal totalAmount = 0;

        foreach (var item in message.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);

            if (product is null)
            {
                reservationFailed = true;
                failureReason = $"Ürün bulunamadı (ProductId: {item.ProductId}).";
                break;
            }

            try
            {
                // Invariant metot void döner, yetersiz stokta InvalidOperationException fırlatır
                product.ReserveStock(item.Quantity);
                totalAmount += product.Price * item.Quantity;
            }
            catch (Exception ex)
            {
                reservationFailed = true;
                failureReason = ex.Message;
                break;
            }
        }

        if (reservationFailed)
        {
            // SaveChangesAsync çağrılmadığı için bellekteki nesne değişiklikleri çöpe atılır
            await context.Publish(new StockReservationFailedEvent(message.OrderId, failureReason));
        }
        else
        {
            await _dbContext.SaveChangesAsync();
            await context.Publish(new StockReservedEvent(message.OrderId, totalAmount));
        }
    }
}