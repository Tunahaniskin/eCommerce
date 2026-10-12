using System;

namespace ECommerce.API.Modules.Order.Features.RefundOrder;

public record RefundOrderCommand(Guid OrderId);
