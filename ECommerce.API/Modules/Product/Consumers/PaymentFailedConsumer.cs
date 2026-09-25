using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Product.Consumers;

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
        _logger.LogWarning("[PRODUCT SAGA] Sipariş {OrderId} için ödeme başarısız. Rezerve stoklar iade ediliyor.", message.OrderId);

        // İade edilecek ürünleri ve miktarları bulmak için sipariş kalemlerini çekiyoruz.
        var orderItems = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == message.OrderId)
            .SelectMany(o => o.Items)
            .ToListAsync();

        foreach (var item in orderItems)
        {
            // Atomik SQL Update: UPDATE Products SET ReservedStock = ReservedStock - Quantity WHERE Id = ProductId
            await _dbContext.Products
                .Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.ReservedStock, p => p.ReservedStock - item.Quantity));
        }

        _logger.LogInformation("[PRODUCT SAGA] Sipariş {OrderId} için rezerve stoklar başarıyla serbest bırakıldı.", message.OrderId);
    }
}
