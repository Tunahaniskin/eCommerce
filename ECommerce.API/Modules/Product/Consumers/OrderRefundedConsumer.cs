using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Logging;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Product.Consumers;

public class OrderRefundedConsumer : IConsumer<OrderRefundedEvent>
{
    private readonly AppDbContext _dbContext;

    public OrderRefundedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<OrderRefundedEvent> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();
        var consumerName = nameof(OrderRefundedConsumer);

        var isProcessed = await _dbContext.ProcessedMessages
            .AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName);

        if (isProcessed) return;

        foreach (var item in context.Message.Items)
        {
            await _dbContext.Products
                .Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
                    
            AppLogger.Info($"[PRODUCT SAGA] İade edildi: Ürün {item.ProductId} stoğuna {item.Quantity} eklendi.");
        }

        await _dbContext.ProcessedMessages.AddAsync(new Infrastructure.Database.Entities.ProcessedMessage(messageId, consumerName));
        await _dbContext.SaveChangesAsync();
    }
}
