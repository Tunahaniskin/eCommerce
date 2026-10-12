using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Finance.Features.GetDepositTickets;

public class GetDepositTicketsEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/finance/deposit-requests", async (
            string? status, 
            IQueryHandler<GetDepositTicketsQuery, List<DepositTicketDto>> handler,
            UserPermissionService permissionService,
            HttpContext context) =>
        {
            if (!await permissionService.HasPermissionAsync(context.User, Permissions.Finance.View))
                return Results.Forbid();

            var query = new GetDepositTicketsQuery(status);
            var result = await handler.HandleAsync(query);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Admin - Finance")
        .Produces<List<DepositTicketDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
