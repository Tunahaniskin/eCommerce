using ECommerce.API.Infrastructure.Handlers;

namespace ECommerce.API.Modules.Product.Features.GetProductById;

public record GetProductByIdQuery(Guid Id, bool CanReadDeleted);
