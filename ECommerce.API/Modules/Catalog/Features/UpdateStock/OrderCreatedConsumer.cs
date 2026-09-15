using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Features.UpdateStock;

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

        foreach (var item in message.Items)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product is null)
            {
                // Sektör standardı: Ürün bulunamazsa loglanır veya telafi (compensating) mekanizması tetiklenir
                continue;
            }

            // Domain Entity içindeki kuralımızı çalıştırıyoruz (Encapsulation)
            product.DecreaseStock(item.Quantity);
        }

        await _dbContext.SaveChangesAsync();
    }
}