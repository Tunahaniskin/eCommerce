namespace ECommerce.API.Shared.Contracts;

public record OrderItemDto(Guid ProductId, int Quantity);
public record CreateOrderItemMessage(Guid ProductId, int Quantity, decimal Price);

// CreateOrderMessage artık fiyatı da taşıyan CreateOrderItemMessage listesini alsın
public record CreateOrderMessage(Guid OrderId, List<CreateOrderItemMessage> Items);

// Katalog rezerve ederken sadece miktar ve ID yeterli olduğu için OrderItemDto kalabilir
public record OrderCreatedEvent(Guid OrderId, List<OrderItemDto> Items);

public record StockReservedEvent(Guid OrderId, decimal TotalAmount);
public record StockReservationFailedEvent(Guid OrderId, string Reason);
public record PaymentSucceededEvent(Guid OrderId);
public record PaymentFailedEvent(Guid OrderId, string Reason);
public record ProductDeletedEvent(Guid ProductId);