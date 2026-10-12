using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace ECommerce.API.Modules.Finance.Features.RejectDepositTicket;

public class RejectDepositTicketEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/finance/deposit-requests/{id:guid}/reject", async (
            Guid id, 
            RejectDepositTicketCommand command,
            ICommandHandler<RejectDepositTicketCommand, IResult> handler,
            UserPermissionService permissionService,
            HttpContext context) =>
        {
            if (!await permissionService.HasPermissionAsync(context.User, Permissions.Finance.Manage))
                return Results.Forbid();

            var newCommand = new RejectDepositTicketCommand(id, command.Reason);
            return await handler.HandleAsync(newCommand);
        })
        .RequireAuthorization()
        .WithTags("Admin - Finance")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
