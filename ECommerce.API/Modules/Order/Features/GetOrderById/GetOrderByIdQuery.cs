using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Order.Features.GetOrderById;

public record GetOrderByIdQuery(Guid Id);

public record OrderDetailsDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    string ShippingAddress,
    List<OrderDetailsItemDto> Items
);

public record OrderDetailsItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
