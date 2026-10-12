using System;

namespace ECommerce.API.Modules.Product.Features.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name, string? Description);
public record UpdateCategoryResult(Guid Id, string Message);
