using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest request, 
            ICommandHandler<CreateOrderCommand, IResult> handler) =>
        {
            if (request.Items == null || !request.Items.Any())
                return Results.BadRequest(new { Message = "Sipariş kalemleri boş olamaz." });
            
            foreach (var item in request.Items)
            {
                if (item.ProductId == Guid.Empty)
                    return Results.BadRequest(new { Message = "Ürün ID geçersiz." });
                if (item.Quantity <= 0)
                    return Results.BadRequest(new { Message = "Miktar 0'dan büyük olmalıdır." });
            }

            // 1. İstek (Request) verisini Komut (Command) verisine dönüştür
            var commandItems = request.Items
                .Select(i => new OrderItemCommand(i.ProductId, i.Quantity))
                .ToList();
                
            var command = new CreateOrderCommand(commandItems);

            // 2. Komutu Handler'a gönder (İş mantığı çalışsın) ve sonucu direkt dön
            return await handler.HandleAsync(command);
        });
    }
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity);