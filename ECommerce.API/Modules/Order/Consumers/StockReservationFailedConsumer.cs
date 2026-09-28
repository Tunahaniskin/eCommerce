using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Order.Consumers;

public class StockReservationFailedConsumer : IConsumer<StockReservationFailedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StockReservationFailedConsumer> _logger;

    public StockReservationFailedConsumer(AppDbContext dbContext, ILogger<StockReservationFailedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogWarning("[ORDER SAGA] Sipariş {OrderId} için stok rezervasyonu başarısız oldu. Sipariş iptal ediliyor. Sebep: {Reason}", message.OrderId, message.Reason);

        var order = await _dbContext.Orders.FindAsync(message.OrderId);

        // Idempotency (Mükerrerlik) Koruması: Sadece Pending durumundaysa Cancelled yap.
        if (order != null && order.Status == OrderStatus.Pending)
        {
            order.Cancel(message.Reason);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("[ORDER SAGA] Sipariş {OrderId} statüsü stok yetersizliği nedeniyle 'Cancelled' olarak güncellendi.", message.OrderId);
        }
        else
        {
            _logger.LogInformation("[ORDER SAGA] Sipariş {OrderId} bulunamadı veya statüsü zaten güncellenmiş (Idempotency). Mevcut statü: {Status}", message.OrderId, order?.Status);
        }
    }
}
