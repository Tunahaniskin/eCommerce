using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Validation;
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
            // Aynı ProductId'lerin miktarlarını toplayarak birleştir
            var consolidatedItems = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new OrderItemRequest(g.Key, g.Sum(x => x.Quantity)))
                .ToList();

            // Toplama işlemi sonrası genel adet sınırını tekrar kontrol et (Opsiyonel ama güvenli)
            if (consolidatedItems.Any(i => i.Quantity > 50))
                return Results.BadRequest(new { Message = "Aynı üründen toplamda en fazla 50 adet sipariş verilebilir." });

            var requestedProductIds = consolidatedItems.Select(i => i.ProductId).ToList();

            // Fiyat manipülasyonunu engellemek için doğrudan Catalog şemasından doğrula
            var products = await dbContext.Products
                .AsNoTracking()
                .Where(p => requestedProductIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Price })
                .ToListAsync();

            if (products.Count != requestedProductIds.Count)
                return Results.BadRequest(new { Message = "Sepetteki bazı ürünler katalogda bulunamadı." });

            var productPriceMap = products.ToDictionary(p => p.Id, p => p.Price);
            var orderId = Guid.NewGuid();

            var orderItems = consolidatedItems.Select(item => new CreateOrderItemMessage(
                item.ProductId,
                item.Quantity,
                productPriceMap[item.ProductId]
            )).ToList();

            await publishEndpoint.Publish(new CreateOrderMessage(orderId, orderItems));

            return Results.Accepted($"/api/order/orders/{orderId}/status", new
            {
                OrderId = orderId,
                Message = "Sipariş işleme alındı."
            });
        })
        .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
    }
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity);