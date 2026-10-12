using System;

namespace ECommerce.API.Modules.Product.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id, Guid? TargetCategoryId);
public record DeleteCategoryResult(bool Success, string Message);
