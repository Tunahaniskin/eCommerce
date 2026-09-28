using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public record GetProductsQuery(int Page, int PageSize, string? Status, bool CanReadDeleted);
