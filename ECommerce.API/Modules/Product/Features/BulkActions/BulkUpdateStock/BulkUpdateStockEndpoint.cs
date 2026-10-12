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

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdateStock;

public class BulkUpdateStockEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/bulk-update-stock", async (
            [FromBody] BulkUpdateStockRequest request,
            ICommandHandler<BulkUpdateStockCommand, BulkUpdateStockResult> handler) =>
        {
            try
            {
                var command = new BulkUpdateStockCommand(request.ProductIds ?? new List<Guid>(), request.Action, request.Value);
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

public record BulkUpdateStockRequest(List<Guid> ProductIds, string Action, int Value);
