namespace ECommerce.API.Modules.Product.Features.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, int Stock, Guid CategoryId);
public record UpdateProductResult(Guid Id, string Message);
