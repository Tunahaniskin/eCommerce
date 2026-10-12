using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Features.Categories.DeleteCategory;

public class DeleteCategoryEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{id:guid}", async (
            Guid id,
            [FromQuery] Guid? targetCategoryId,
            ICommandHandler<DeleteCategoryCommand, DeleteCategoryResult> handler) =>
        {
            try
            {
                var command = new DeleteCategoryCommand(id, targetCategoryId);
                var result = await handler.HandleAsync(command);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithTags("Admin - Categories")
        .Produces<DeleteCategoryResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
