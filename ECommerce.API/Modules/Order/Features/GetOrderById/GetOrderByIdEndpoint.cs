using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Order.Features.GetOrderById;

public class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id:guid}", async (
            Guid id,
            IQueryHandler<GetOrderByIdQuery, OrderDetailsDto?> handler,
            UserPermissionService permissionService,
            HttpContext context) =>
        {
            var isCustomer = context.User.Identity?.IsAuthenticated == true;
            var isAdmin = await permissionService.HasPermissionAsync(context.User, Permissions.Order.ViewAll);

            if (!isCustomer && !isAdmin)
                return Results.Forbid();

            var query = new GetOrderByIdQuery(id);
            var result = await handler.HandleAsync(query);

            if (result == null)
                return Results.NotFound();

            if (!isAdmin)
            {
                var callerIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(callerIdStr, out var callerId) || result.UserId != callerId)
                {
                    return Results.Forbid(); // Müşteri sadece kendi siparişinin detayını görebilir
                }
            }

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Orders")
        .Produces<OrderDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
