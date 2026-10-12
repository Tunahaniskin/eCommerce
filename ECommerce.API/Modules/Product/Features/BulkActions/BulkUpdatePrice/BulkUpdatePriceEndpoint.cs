using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdatePrice;

public class BulkUpdatePriceEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/bulk-update-price", async (
            [FromBody] BulkUpdatePriceRequest request,
            ICommandHandler<BulkUpdatePriceCommand, BulkUpdatePriceResult> handler) =>
        {
            try
            {
                var command = new BulkUpdatePriceCommand(request.ProductIds ?? new List<Guid>(), request.Action, request.Value);
                var result = await handler.HandleAsync(command);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.Product.Update)
        .WithTags("Admin - Product Bulk Actions");
    }
}

public record BulkUpdatePriceRequest(List<Guid> ProductIds, string Action, decimal Value);
