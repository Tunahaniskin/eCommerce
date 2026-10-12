using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;

namespace ECommerce.API.Modules.Order.Features.RefundOrder;

public class RefundOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{id:guid}/refund", async (
            Guid id, 
            ICommandHandler<RefundOrderCommand, IResult> handler,
            UserPermissionService permissionService,
            HttpContext context) =>
        {
            var isCustomer = context.User.Identity?.IsAuthenticated == true;
            var isAdminWithCancelPerm = await permissionService.HasPermissionAsync(context.User, Permissions.Order.Cancel);

            if (!isCustomer && !isAdminWithCancelPerm)
                return Results.Forbid();

            var command = new RefundOrderCommand(id);
            return await handler.HandleAsync(command);
        })
        .RequireAuthorization()
        .WithTags("Orders")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}
