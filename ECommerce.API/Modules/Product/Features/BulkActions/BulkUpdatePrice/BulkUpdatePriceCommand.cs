using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Features.BulkActions.BulkUpdatePrice;

public record BulkUpdatePriceCommand(List<Guid> ProductIds, string Action, decimal Value);
public record BulkUpdatePriceResult(int SuccessCount, string Message);
