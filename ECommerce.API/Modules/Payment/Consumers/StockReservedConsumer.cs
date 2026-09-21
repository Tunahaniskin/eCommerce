using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Payment.Consumers;

public class StockReservedConsumer : IConsumer<StockReservedEvent>
{
    // IPublishEndpoint kaldırıldı, constructor'a gerek kalmadı

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var message = context.Message;

        Console.WriteLine($"[PAYMENT MODULU] Gelen OrderId: {message.OrderId}, Okunan TotalAmount: {message.TotalAmount}");

        bool isPaymentSuccessful = message.TotalAmount <= 50000;

        if (isPaymentSuccessful)
        {
            await context.Publish(new PaymentSucceededEvent(message.OrderId));
        }
        else
        {
            await context.Publish(new PaymentFailedEvent(message.OrderId, "Bakiye yetersiz veya kart reddedildi."));
        }
    }
}