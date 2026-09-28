using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.Modules.Product.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(AppDbContext dbContext, ILogger<OrderCreatedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        bool reservationFailed = false;
        string failureReason = string.Empty;
        decimal totalAmount = 0;

        // 1. Transaction Başlat (All-or-Nothing kuralı için)
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in message.Items)
            {
                // Fiyatı hesaplamak için ürünü NoTracking ile sadece okuyoruz
                var product = await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product is null)
                {
                    reservationFailed = true;
                    failureReason = $"Ürün bulunamadı (ProductId: {item.ProductId}).";
                    break; // Döngüden çık, işlem iptal edilecek
                }

                _logger.LogInformation("[PRODUCT SAGA] Ürün {ProductId} için {Stock} adet stok var.", item.ProductId, product.Stock);

                // 2. Atomik SQL Update ile stok rezerve etme (Race condition önleme)
                // Yalnızca mevcut stok (Stock - ReservedStock) istenen miktardan büyük/eşitse günceller
                var affectedRows = await _dbContext.Products
                    .Where(p => p.Id == item.ProductId && p.Stock - p.ReservedStock >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.ReservedStock, p => p.ReservedStock + item.Quantity));

                if (affectedRows == 0)
                {
                    reservationFailed = true;
                    failureReason = $"'{product.Name}' isimli ürün için yetersiz stok veya eşzamanlı işlem çakışması.";
                    break; // Döngüden çık, işlem iptal edilecek
                }

                _logger.LogInformation("[PRODUCT SAGA] Ürün {ProductId} için {Quantity} adet stok rezerve edildi.", item.ProductId, item.Quantity);

                totalAmount += product.Price * item.Quantity;
            }

            if (reservationFailed)
            {
                // Stok yetersizliği veya bulunamama durumunda tüm değişiklikleri geri al (Rollback)
                await transaction.RollbackAsync();
                
                // Order modülüne siparişi iptal etmesi için event fırlat
                await context.Publish(new StockReservationFailedEvent(message.OrderId, failureReason));
            }
            else
            {
                // Tüm ürünler başarıyla rezerve edildi, kalıcı yap (Commit)
                await transaction.CommitAsync();
                
                // Başarılı olduğuna dair eventi ödeme modülüne fırlat
                await context.Publish(new StockReservedEvent(message.OrderId, totalAmount));
            }
        }
        catch (Exception ex)
        {
            // Beklenmedik bir hata olursa veritabanını geri al ve fail fırlat
            await transaction.RollbackAsync();
            await context.Publish(new StockReservationFailedEvent(message.OrderId, "Stok rezervasyonu sırasında sistemsel bir hata oluştu: " + ex.Message));
        }
    }
}
