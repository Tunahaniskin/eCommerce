using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, IResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        // Aynı ProductId'lerin miktarlarını toplayarak birleştir
        var consolidatedItems = command.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderItemCommand(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        // Toplama işlemi sonrası genel adet sınırını kontrol et
        if (consolidatedItems.Any(i => i.Quantity > 50))
            return Results.BadRequest(new { Message = "Aynı üründen toplamda en fazla 50 adet sipariş verilebilir." });

        var requestedProductIds = consolidatedItems.Select(i => i.ProductId).ToList();

        // Fiyat manipülasyonunu engellemek için doğrudan Product şemasından doğrula
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => requestedProductIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Price })
            .ToListAsync(cancellationToken);

        if (products.Count != requestedProductIds.Count)
            return Results.BadRequest(new { Message = "Sepetteki bazı ürünler katalogda bulunamadı." });

        var productPriceMap = products.ToDictionary(p => p.Id, p => p.Price);
        var orderId = Guid.NewGuid();

        var orderItems = consolidatedItems.Select(item => new CreateOrderItemMessage(
            item.ProductId,
            item.Quantity,
            productPriceMap[item.ProductId]
        )).ToList();

        await _publishEndpoint.Publish(new CreateOrderMessage(orderId, orderItems), cancellationToken);

        return Results.Accepted($"/api/order/orders/{orderId}/status", new
        {
            OrderId = orderId,
            Message = "Sipariş işleme alındı."
        });
    }
}
