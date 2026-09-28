using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using ECommerce.API.Infrastructure.Logging;

namespace ECommerce.API.Modules.Order.Consumers;

public class StockReservationFailedConsumer : IConsumer<StockReservationFailedEvent>
{
    private readonly AppDbContext _dbContext;
    public StockReservationFailedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
    {
        var message = context.Message;
        
        AppLogger.Warn($"[ORDER SAGA] Sipariş {message.OrderId} için stok rezervasyonu başarısız oldu. Sipariş iptal ediliyor. Sebep: {message.Reason}");

        var order = await _dbContext.Orders.FindAsync(message.OrderId);

        // Idempotency (Mükerrerlik) Koruması: Sadece Pending durumundaysa Cancelled yap.
        if (order != null && order.Status == OrderStatus.Pending)
        {
            order.Cancel(message.Reason);
            await _dbContext.SaveChangesAsync();
            
            AppLogger.Info($"[ORDER SAGA] Sipariş {message.OrderId} statüsü stok yetersizliği nedeniyle 'Cancelled' olarak güncellendi.");
        }
        else
        {
            AppLogger.Info($"[ORDER SAGA] Sipariş {message.OrderId} bulunamadı veya statüsü zaten güncellenmiş (Idempotency). Mevcut statü: {order?.Status}");
        }
    }
}
