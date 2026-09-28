using ECommerce.API.Infrastructure.Logging;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

[Loggable("Müşteri Siparişi Oluşturma", AppLogLevel.Info)]
[PerformanceThreshold(300)]
public record CreateOrderCommand(List<OrderItemCommand> Items);

public record OrderItemCommand(Guid ProductId, int Quantity);
