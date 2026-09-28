using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Validation;
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
            // 1. İstek (Request) verisini Komut (Command) verisine dönüştür
            var commandItems = request.Items
                .Select(i => new OrderItemCommand(i.ProductId, i.Quantity))
                .ToList();
                
            var command = new CreateOrderCommand(commandItems);

            // 2. Komutu Handler'a gönder (İş mantığı çalışsın) ve sonucu direkt dön
            return await handler.HandleAsync(command);
        })
        .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
    }
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity);