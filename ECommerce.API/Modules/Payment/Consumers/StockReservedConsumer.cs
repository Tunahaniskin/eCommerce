using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Modules.Payment.Entities;
using ECommerce.API.Modules.Finance.Entities;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Payment.Consumers;

public class StockReservedConsumer : IConsumer<StockReservedEvent>
{
    private readonly AppDbContext _dbContext;

    public StockReservedConsumer(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();
        var consumerName = nameof(StockReservedConsumer);

        // 1. Idempotency Check (Mükerrer mesaj kontrolü)
        var isProcessed = await _dbContext.ProcessedMessages
            .AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName);

        if (isProcessed)
        {
            Console.WriteLine($"[PAYMENT] Message {messageId} already processed, skipping.");
            return;
        }

        // 2. Business Idempotency
        var existingSuccessfulPayment = await _dbContext.Payments
            .AnyAsync(p => p.OrderId == context.Message.OrderId && p.Status == PaymentStatus.Success);

        if (existingSuccessfulPayment)
        {
            Console.WriteLine($"[PAYMENT] Order {context.Message.OrderId} already paid, skipping.");
            await _dbContext.ProcessedMessages.AddAsync(new ProcessedMessage(messageId, consumerName));
            await _dbContext.SaveChangesAsync();
            return;
        }

        Console.WriteLine($"[PAYMENT] Processing payment for Order: {context.Message.OrderId}, Amount: {context.Message.TotalAmount}");

        // 3. Payment Kaydını Oluştur (Pending olarak)
        var payment = new Entities.Payment(context.Message.OrderId, context.Message.TotalAmount);
        await _dbContext.Payments.AddAsync(payment);

        // 4. Cüzdan (Wallet) Bakiyesini Kontrol Et
        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == context.Message.UserId);

        if (wallet == null || wallet.Balance < context.Message.TotalAmount)
        {
            // Bakiye Yetersiz
            payment.MarkAsFailed("Yetersiz cüzdan bakiyesi.");
            await context.Publish(new PaymentFailedEvent(payment.OrderId, "Yetersiz cüzdan bakiyesi."));
        }
        else
        {
            // Bakiye Yeterli
            wallet.DeductBalance(context.Message.TotalAmount);
            
            var transaction = new WalletTransaction(
                wallet.Id, 
                context.Message.TotalAmount, 
                WalletTransactionType.OrderPayment, 
                $"Sipariş ({context.Message.OrderId}) ödemesi.");
            await _dbContext.WalletTransactions.AddAsync(transaction);

            var transactionId = $"WLT-{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}";
            payment.MarkAsSuccess(transactionId);

            await context.Publish(new PaymentSucceededEvent(payment.OrderId));
        }

        // 6. Mesajı işlendi olarak işaretle
        await _dbContext.ProcessedMessages.AddAsync(new ProcessedMessage(messageId, consumerName));
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"[PAYMENT] Payment {payment.Status} for Order: {context.Message.OrderId}");
    }
}