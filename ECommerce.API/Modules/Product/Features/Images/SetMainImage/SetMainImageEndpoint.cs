using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Images.SetMainImage;

public class SetMainImageEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Frontend route: /api/admin/products/{productId}/images/{imageId}/set-main
        app.MapPut("/products/{productId:guid}/images/{imageId:guid}/set-main", async (
            Guid productId,
            Guid imageId,
            ICommandHandler<SetMainImageCommand, SetMainImageResult> handler) =>
        {
            try
            {
                var command = new SetMainImageCommand(productId, imageId);
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

        // Alternative route: /api/admin/products/{productId}/images/{imageId}/main
        app.MapPut("/products/{productId:guid}/images/{imageId:guid}/main", async (
            Guid productId,
            Guid imageId,
            ICommandHandler<SetMainImageCommand, SetMainImageResult> handler) =>
        {
            try
            {
                var command = new SetMainImageCommand(productId, imageId);
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
