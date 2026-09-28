using ECommerce.API.Infrastructure.Endpoints;
using ECommerce.API.Infrastructure.Extensions;
using ECommerce.API.Infrastructure.Validation;
using ECommerce.API.Modules.Auth.Constants;
using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public class CreateProductEndpoint : IAdminEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ICommandHandler<CreateProductCommand, CreateProductResult> handler) =>
        {
            var command = new CreateProductCommand(request.Name, request.Price, request.Stock);
            var result = await handler.HandleAsync(command);

            return Results.Created($"/api/products/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateProductRequest>>()
        .RequirePermission(Permissions.Product.Create);
    }
}

public record CreateProductRequest(string Name, decimal Price, int Stock);
