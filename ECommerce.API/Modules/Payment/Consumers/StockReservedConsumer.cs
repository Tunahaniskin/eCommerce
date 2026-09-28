using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using ECommerce.API.Modules.Payment.Entities;
using ECommerce.API.Modules.Payment.Providers;
using ECommerce.API.Shared.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Payment.Consumers;

public class StockReservedConsumer : IConsumer<StockReservedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;

    public StockReservedConsumer(AppDbContext dbContext, IPaymentGateway paymentGateway)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
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
        await _dbContext.SaveChangesAsync(); 

        // 4. Payment Gateway'e İstek At
        var result = await _paymentGateway.ProcessPaymentAsync(payment.OrderId, payment.Amount, payment.Currency, context.CancellationToken);

        // 5. Sonuca göre durumu güncelle ve Event fırlat
        if (result.IsSuccess)
        {
            payment.MarkAsSuccess(result.TransactionId!);

            await context.Publish(new PaymentSucceededEvent(payment.OrderId));
        }
        else
        {
            payment.MarkAsFailed(result.ErrorMessage ?? "Payment Failed");

            await context.Publish(new PaymentFailedEvent(payment.OrderId, result.ErrorMessage ?? "Payment Failed"));
        }

        // 6. Mesajı işlendi olarak işaretle
        await _dbContext.ProcessedMessages.AddAsync(new ProcessedMessage(messageId, consumerName));
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"[PAYMENT] Payment {payment.Status} for Order: {context.Message.OrderId}");
    }
}