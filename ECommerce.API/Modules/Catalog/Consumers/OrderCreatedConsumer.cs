using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Catalog.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedConsumer(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        bool reservationFailed = false;
        decimal totalAmount = 0;

        // 1. Siparişteki tüm ürünleri tek tek dönüp rezerve etmeye çalışıyoruz
        foreach (var item in message.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            
            // Ürün yoksa veya ReserveStock(miktar) false dönerse (stok yetersizse)
            if (product is null || !product.ReserveStock(item.Quantity))
            {
                reservationFailed = true;
                break; // Döngüyü kır, diğer ürünlere bakmaya gerek yok
            }
            
            totalAmount += product.Price * item.Quantity;
        }

        // 2. Kontrol ve Sonuç Fırlatma
        if (reservationFailed)
        {
            // Hata durumunda _dbContext.SaveChangesAsync() ÇAĞIRMIYORUZ. 
            // Böylece o ana kadar başarıyla rezerve edilmiş ürünler varsa bile EF Core onları veritabanına yazmadan çöpe atar (Memory Rollback).
            await _publishEndpoint.Publish(new StockReservationFailedEvent(message.OrderId, "Stok yetersiz veya ürün bulunamadı."));
        }
        else
        {
            // Tüm ürünler rezerve edildiyse veritabanına kaydet.
            await _dbContext.SaveChangesAsync();
            
            // Ödeme modülünün dinlemesi için başarılı rezervasyon olayını fırlat.
            await _publishEndpoint.Publish(new StockReservedEvent(message.OrderId, totalAmount));
        }
    }
}