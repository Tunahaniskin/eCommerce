using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

// Dışarıdan gelen HTTP istek gövdesi (Request DTO)
public record CreateOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public record CreateOrderRequest(List<CreateOrderItemRequest> Items);

public static class CreateOrderEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/order/orders", async (
            CreateOrderRequest request, 
            AppDbContext dbContext, 
            IPublishEndpoint publishEndpoint) =>
        {
            // 1. Gelen DTO'yu Domain Entity'lerine dönüştürüyoruz
            var orderItems = request.Items.Select(item => 
                new OrderItem(item.ProductId, item.Quantity, item.UnitPrice)
            ).ToList();

            // 2. Sipariş nesnesini oluşturuyoruz (Toplam tutar kuralları burada işletilir)
            var order = new ECommerce.API.Modules.Order.Entities.Order(orderItems);

            // 3. Siparişi veritabanına ekliyoruz
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            // 4. RabbitMQ'ya Event fırlatıyoruz (Publish)
            // Order modülü Catalog modülünü doğrudan çağırmaz; sadece olayı anons eder.
            var eventItems = order.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity)).ToList();
            await publishEndpoint.Publish(new OrderCreatedEvent(order.Id, eventItems));

            // 5. Yanıtı dönüyoruz
            return Results.Ok(new 
            { 
                order.Id, 
                order.TotalAmount, 
                Message = "Sipariş alındı ve stok güncelleme kuyruğa iletildi." 
            });
        });
    }
}