using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Finance.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Finance.Consumers;

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

        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == context.Message.UserId);

        if (wallet != null)
        {
            wallet.AddBalance(context.Message.RefundAmount);

            var transaction = new WalletTransaction(
                wallet.Id, 
                context.Message.RefundAmount, 
                WalletTransactionType.OrderRefund, 
                $"Sipariş ({context.Message.OrderId}) iadesi.");
                
            await _dbContext.WalletTransactions.AddAsync(transaction);
        }

        await _dbContext.ProcessedMessages.AddAsync(new Infrastructure.Database.Entities.ProcessedMessage(messageId, consumerName));
        await _dbContext.SaveChangesAsync();
    }
}
