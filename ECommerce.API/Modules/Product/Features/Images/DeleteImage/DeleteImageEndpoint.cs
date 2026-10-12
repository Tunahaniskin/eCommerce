using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.DeleteImage;

public class DeleteImageEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Frontend route: /api/admin/products/{productId}/images/{imageId}
        app.MapDelete("/products/{productId:guid}/images/{imageId:guid}", async (
            Guid productId,
            Guid imageId,
            ICommandHandler<DeleteImageCommand, DeleteImageResult> handler) =>
        {
            try
            {
                var command = new DeleteImageCommand(imageId);
                var result = await handler.HandleAsync(command);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.Product.Update)
        .WithTags("Admin - Product Images");

        // Alternative route: /api/admin/products/images/{imageId}
        app.MapDelete("/products/images/{imageId:guid}", async (
            Guid imageId,
            ICommandHandler<DeleteImageCommand, DeleteImageResult> handler) =>
        {
            try
            {
                var command = new DeleteImageCommand(imageId);
                var result = await handler.HandleAsync(command);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .RequirePermission(Permissions.Product.Update)
        .WithTags("Admin - Product Images");
    }
}
