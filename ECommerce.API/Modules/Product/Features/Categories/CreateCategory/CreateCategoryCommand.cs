using System;

namespace ECommerce.API.Modules.Product.Features.Categories.CreateCategory;

public record CreateCategoryCommand(string Name, string? Description);
public record CreateCategoryResult(Guid Id, string Message);
