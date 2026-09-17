using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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
            if (request.Items == null || !request.Items.Any())
                return Results.BadRequest(new { Message = "Sipariş boş olamaz." });

            // 1. Sadece Varlık Kontrolü (İstemciden gelen fiyatlara güvenmiyoruz, sadece ID kontrolü)
            var productIds = request.Items.Select(x => x.ProductId).ToList();
            var existingProducts = await dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (existingProducts.Count != productIds.Count)
                return Results.BadRequest(new { Message = "Sepetteki bazı ürünler sistemde bulunamadı." });

            // 2. Sipariş ID'sini üret ve Komutu Havuza Bırak
            var orderId = Guid.NewGuid();
            var message = new CreateOrderMessage(
                orderId, 
                request.Items.Select(x => new OrderItemDto(x.ProductId, x.Quantity)).ToList()
            );

            await publishEndpoint.Publish(message);

            // 3. İstemciye 202 Accepted dön
            return Results.Accepted($"/api/order/orders/{orderId}", new 
            { 
                OrderId = orderId, 
                Message = "Siparişiniz sıraya alındı, arka planda işleniyor." 
            });
        });
    }
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
// DİKKAT: UnitPrice alanını istemciden almayı tamamen kopardık.
public record OrderItemRequest(Guid ProductId, int Quantity);