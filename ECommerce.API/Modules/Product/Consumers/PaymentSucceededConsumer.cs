using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.Modules.Product.Consumers;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PaymentSucceededConsumer> _logger;

    public PaymentSucceededConsumer(AppDbContext dbContext, ILogger<PaymentSucceededConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();
        var consumerName = GetType().FullName!;

        // 1. Idempotency (Mükerrerlik) Kontrolü
        var alreadyProcessed = await _dbContext.ProcessedMessages
            .AnyAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName);

        if (alreadyProcessed)
        {
            _logger.LogWarning("[PRODUCT SAGA] Mükerrer PaymentSucceededEvent tespit edildi. İşlem atlanıyor. MessageId: {MessageId}", messageId);
            return;
        }

        // Siparişi ve kalemlerini çekiyoruz (Tracking'e gerek yok)
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null) return;

        // 2. Transaction Başlat
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Her ürün için rezerve stoğu ve asıl stoğu veritabanında atomik olarak düş (Commit et)
            // foreach (var item in order.Items)
            // {
            //     await _dbContext.Products
            //         .Where(p => p.Id == item.ProductId)
            //         .ExecuteUpdateAsync(setters => setters
            //             .SetProperty(p => p.Stock, p => p.Stock - item.Quantity)
            //             .SetProperty(p => p.ReservedStock, p => p.ReservedStock - item.Quantity));
                        
            //     _logger.LogInformation("[PRODUCT SAGA] Ürün {ProductId} için {Quantity} adet stok sistemden kalıcı olarak düşüldü.", item.ProductId, item.Quantity);
            // }

            foreach (var item in order.Items)
            {
                var product = await _dbContext.Products.FindAsync(item.ProductId); // <-- BURASI!
                if (product is not null)
                {
                    product.CommitStock(item.Quantity);
                }
            }
           


            // 3. Mesajı işlendi olarak kaydet
            _dbContext.ProcessedMessages.Add(new ProcessedMessage(messageId, consumerName));

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            _logger.LogInformation("[PRODUCT SAGA] Sipariş {OrderId} için stok kalıcı olarak düşüldü.", message.OrderId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "[PRODUCT SAGA] Stok kalıcı hale getirilirken hata oluştu. Sipariş: {OrderId}", message.OrderId);
            throw;
        }
    }
}
