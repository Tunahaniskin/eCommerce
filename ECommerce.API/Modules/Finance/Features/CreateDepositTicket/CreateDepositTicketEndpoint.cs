using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.API.Modules.Finance.Features.CreateDepositTicket;

public class CreateDepositTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/finance/deposit-requests", async (CreateDepositTicketCommand command, ICommandHandler<CreateDepositTicketCommand, IResult> handler) =>
        {
            return await handler.HandleAsync(command);
        })
        .RequireAuthorization()
        .WithTags("Wallet")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
