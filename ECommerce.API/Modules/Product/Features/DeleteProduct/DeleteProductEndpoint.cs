using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.DeleteProduct;

public class DeleteProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:guid}", async (Guid id, ICommandHandler<DeleteProductCommand, DeleteProductResult> handler) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await handler.HandleAsync(command);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("Geçersiz"))
                    return Results.BadRequest(new { result.Message });
                    
                return Results.NotFound(new { result.Message });
            }

            return Results.Ok(new { result.Message });
        })
        .RequirePermission(Permissions.Product.Delete);
    }
}
