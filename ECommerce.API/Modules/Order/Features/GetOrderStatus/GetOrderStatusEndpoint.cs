using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Order.Features.GetOrderStatus;

public class GetOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}/status", async (
            Guid id,
            IQueryHandler<GetOrderStatusQuery, IResult> handler) =>
        {
            var query = new GetOrderStatusQuery(id);
            return await handler.HandleAsync(query);
        });
    }
}