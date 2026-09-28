using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.DeleteProduct;

public record DeleteProductCommand(Guid Id);

public record DeleteProductResult(bool IsSuccess, string Message);
