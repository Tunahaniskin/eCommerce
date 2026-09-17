using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/order/orders", async (
            CreateOrderRequest request, 
            AppDbContext dbContext, 
            IPublishEndpoint publishEndpoint) =>
        {
            // 1. Önce OrderItem listesini oluşturuyoruz
            var orderItems = request.Items
                .Select(item => new OrderItem(item.ProductId, item.Quantity, item.UnitPrice))
                .ToList();

            // 2. Order nesnesini mevcut public kurucu metot üzerinden üretiyoruz
            var order = new Entities.Order(orderItems);

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            // 3. Olayı RabbitMQ'ya fırlatıyoruz
            await publishEndpoint.Publish(new OrderCreatedEvent(
                order.Id,
                order.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity)).ToList()
            ));

            return Results.Ok(new 
            { 
                order.Id, 
                order.TotalAmount, 
                Message = "Sipariş alındı ve stok güncelleme kuyruğa iletildi." 
            });
        });
    }
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);