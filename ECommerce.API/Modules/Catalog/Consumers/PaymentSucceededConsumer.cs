using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Consumers;

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

        // Siparişi ve kalemlerini çekiyoruz
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null) return;

        // Her ürün için rezerve stoğu asıl stoktan düş (Commit et)
        foreach (var item in order.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            if (product is not null)
            {
                product.CommitStock(item.Quantity);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}