using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace ECommerce.API.Modules.Order.Features.GetOrders;

public class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (
            string? search,
            string? status,
            string? startDate,
            string? endDate,
            string? userId,
            IQueryHandler<GetOrdersQuery, List<OrderDto>> handler,
            UserPermissionService permissionService,
            HttpContext context) =>
        {
            var isCustomer = context.User.Identity?.IsAuthenticated == true;
            var isAdmin = await permissionService.HasPermissionAsync(context.User, Permissions.Order.ViewAll);

            if (!isCustomer && !isAdmin)
                return Results.Forbid();

            Guid? targetUserId = null;
            if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var uid))
            {
                targetUserId = uid;
            }

            var callerIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(callerIdStr, out var callerId))
            {
                if (!isAdmin)
                {
                    targetUserId = callerId;
                }
            }
            else
            {
                if (!isAdmin) return Results.Unauthorized();
            }

            DateTime? parsedStartDate = !string.IsNullOrWhiteSpace(startDate) && DateTime.TryParse(startDate, out var sd) ? sd : null;
            DateTime? parsedEndDate = !string.IsNullOrWhiteSpace(endDate) && DateTime.TryParse(endDate, out var ed) ? ed : null;

            var query = new GetOrdersQuery(targetUserId, search, status, parsedStartDate, parsedEndDate);
            var result = await handler.HandleAsync(query);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Orders")
        .Produces<List<OrderDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
