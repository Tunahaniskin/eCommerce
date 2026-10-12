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

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkDelete;

public class BulkDeleteEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/bulk-delete", async (
            [FromBody] BulkDeleteRequest request,
            ICommandHandler<BulkDeleteCommand, BulkDeleteResult> handler) =>
        {
            var command = new BulkDeleteCommand(request.ProductIds ?? new List<Guid>());
            var result = await handler.HandleAsync(command);
            return Results.Ok(result);
        })
        .RequirePermission(Permissions.Product.Delete)
        .WithTags("Admin - Product Bulk Actions");
    }
}

public record BulkDeleteRequest(List<Guid> ProductIds);
