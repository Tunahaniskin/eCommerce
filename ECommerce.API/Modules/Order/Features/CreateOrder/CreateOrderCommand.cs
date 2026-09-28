namespace ECommerce.API.Modules.Order.Features.CreateOrder;

public record CreateOrderCommand(List<OrderItemCommand> Items);

public record OrderItemCommand(Guid ProductId, int Quantity);
