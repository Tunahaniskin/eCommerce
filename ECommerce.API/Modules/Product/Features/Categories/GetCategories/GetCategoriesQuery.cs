using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Features.Categories.GetCategories;

public record CategoryDto(Guid Id, string Name, string? Description, int ProductCount, DateTime CreatedAt);

public record GetCategoriesQuery(int Page = 1, int PageSize = 20);
