using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Finance.Features.AdjustWalletBalance;

public record AdjustWalletBalanceRequest(decimal Amount, string ActionType, string Reason);

public class AdjustWalletBalanceEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:guid}/wallet-adjustment", async (
            Guid userId,
            [FromBody] AdjustWalletBalanceRequest request,
            ICommandHandler<AdjustWalletBalanceCommand, IResult> handler,
            HttpContext context) =>
        {
            var adminIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(adminIdStr, out var adminUserId))
                return Results.Unauthorized();

            try
            {
                var command = new AdjustWalletBalanceCommand(
                    userId,
                    request.Amount,
                    request.ActionType,
                    request.Reason,
                    adminUserId
                );

                return await handler.HandleAsync(command);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message, error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithTags("Admin - Users Wallet")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
