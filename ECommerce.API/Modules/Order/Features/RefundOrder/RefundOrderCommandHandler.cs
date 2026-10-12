using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Order.Features.RefundOrder;

public class RefundOrderCommandHandler : ICommandHandler<RefundOrderCommand, IResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public RefundOrderCommandHandler(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IResult> HandleAsync(RefundOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order == null)
            return Results.NotFound(new { Message = "Sipariş bulunamadı." });

        if (order.Status != OrderStatus.Paid)
            return Results.BadRequest(new { Message = "Yalnızca ödenmiş siparişler iade edilebilir." });

        order.MarkAsRefunded();
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        var orderItemsDto = order.Items.Select(i => new OrderItemDto(i.ProductId, i.Quantity)).ToList();

        await _publishEndpoint.Publish(
            new OrderRefundedEvent(order.Id, order.UserId, order.TotalAmount, orderItemsDto), 
            cancellationToken
        );

        return Results.Ok(new { Message = "İade işlemi başlatıldı." });
    }
}
