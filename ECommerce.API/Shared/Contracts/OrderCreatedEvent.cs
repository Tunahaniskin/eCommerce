namespace ECommerce.API.Shared.Contracts;

public record OrderCreatedEvent(
    Guid OrderId,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid ProductId,
    int Quantity
);