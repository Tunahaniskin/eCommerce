using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkDelete;

public record BulkDeleteCommand(List<Guid> ProductIds);
public record BulkDeleteResult(int SuccessCount, string Message);
