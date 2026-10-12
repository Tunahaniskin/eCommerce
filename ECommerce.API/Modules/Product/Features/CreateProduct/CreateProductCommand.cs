using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.CreateProduct;

using Microsoft.AspNetCore.Http;

public record CreateProductCommand(string Name, decimal Price, int Stock, Guid CategoryId, IFormFileCollection? Images);

public record CreateProductResult(Guid Id, string Message);
