using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdateStock;

public record BulkUpdateStockCommand(List<Guid> ProductIds, string Action, int Value);
public record BulkUpdateStockResult(int SuccessCount, string Message);
