using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

public record CreateProductCommand(string Name, decimal Price, int Stock);

public record CreateProductResult(Guid Id, string Message);
