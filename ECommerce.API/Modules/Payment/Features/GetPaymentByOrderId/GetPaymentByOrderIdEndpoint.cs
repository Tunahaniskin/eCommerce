using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace ECommerce.API.Modules.Payment.Features.GetPaymentByOrderId;

public class GetPaymentByOrderIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/payments/order/{orderId:guid}", async (Guid orderId, IQueryHandler<GetPaymentByOrderIdQuery, PaymentDto?> handler) =>
        {
            var query = new GetPaymentByOrderIdQuery(orderId);
            var result = await handler.HandleAsync(query);

            return result is not null 
                ? Results.Ok(result) 
                : Results.NotFound(new { Message = "Payment not found for this order." });
        })
        .RequireAuthorization()
        .WithTags("Payments")
        .Produces<PaymentDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
