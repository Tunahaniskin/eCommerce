using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Order.Features.GetOrders;

public record GetOrdersQuery(
    Guid? UserId = null,
    string? Search = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record OrderDto(
    Guid Id, 
    string UserEmail, 
    decimal TotalAmount, 
    string Status, 
    DateTime CreatedAt
);
