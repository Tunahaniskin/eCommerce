using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Infrastructure.Logging;

namespace ECommerce.API.Modules.Product.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly AppDbContext _dbContext;
    public PaymentFailedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();
        var consumerName = GetType().FullName!;

        // 1. Idempotency (Mükerrerlik) Kontrolü
        var alreadyProcessed = await _dbContext.ProcessedMessages
            .AnyAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName);

        if (alreadyProcessed)
        {
            AppLogger.Warn($"[PRODUCT SAGA] Mükerrer PaymentFailedEvent tespit edildi. İşlem atlanıyor. MessageId: {messageId}");
            return;
        }

        AppLogger.Warn($"[PRODUCT SAGA] Sipariş {message.OrderId} için ödeme başarısız. Rezerve stoklar iade ediliyor.");

        // İade edilecek ürünleri ve miktarları bulmak için sipariş kalemlerini çekiyoruz.
        var orderItems = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == message.OrderId)
            .SelectMany(o => o.Items)
            .ToListAsync();

        // 2. Transaction Başlat
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in orderItems)
            {
                // Atomik SQL Update: UPDATE Products SET ReservedStock = ReservedStock - Quantity WHERE Id = ProductId
                await _dbContext.Products
                    .Where(p => p.Id == item.ProductId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.ReservedStock, p => p.ReservedStock - item.Quantity));
                        
                AppLogger.Info($"[PRODUCT SAGA] Ödeme başarısız. Ürün {item.ProductId} için {item.Quantity} adet rezerve stok iade edildi.");
            }

            // 3. Mesajı işlendi olarak kaydet
            _dbContext.ProcessedMessages.Add(new ProcessedMessage(messageId, consumerName));

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            AppLogger.Info($"[PRODUCT SAGA] Sipariş {message.OrderId} için rezerve stoklar başarıyla serbest bırakıldı.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            AppLogger.Error($"[PRODUCT SAGA] Rezerve stok iadesi sırasında hata oluştu. Sipariş: {message.OrderId}", ex);
            throw; // MassTransit'in tekrar denemesi (retry) için hatayı fırlat
        }
    }
}
