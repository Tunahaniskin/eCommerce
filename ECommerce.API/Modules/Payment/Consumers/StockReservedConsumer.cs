using ECommerce.API.Shared.Contracts;
using MassTransit;

namespace ECommerce.API.Modules.Payment.Consumers;

public class StockReservedConsumer : IConsumer<StockReservedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public StockReservedConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var message = context.Message;

        // Gelen gerçek tutarı konsola yazdır
        Console.WriteLine($"[PAYMENT MODULU] Gelen OrderId: {message.OrderId}, Okunan TotalAmount: {message.TotalAmount}");

        // Mock Ödeme Senaryosu: 
        // Gerçek bir senaryoda burada dış bir ödeme API'sine gidilir.
        // Biz simülasyon için 50.000 TL sınır koyuyoruz.
        bool isPaymentSuccessful = message.TotalAmount <= 50000;

        if (isPaymentSuccessful)
        {
            // Ödeme çekildi.
            await _publishEndpoint.Publish(new PaymentSucceededEvent(message.OrderId));
        }
        else
        {
            // Kart limiti yetmedi veya banka reddetti.
            await _publishEndpoint.Publish(new PaymentFailedEvent(message.OrderId, "Bakiye yetersiz veya kart reddedildi."));
        }
    }
}