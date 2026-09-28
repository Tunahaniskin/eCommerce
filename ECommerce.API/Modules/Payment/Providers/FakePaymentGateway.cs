using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Modules.Payment.Providers;

public class FakePaymentGateway : IPaymentGateway
{
    public async Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        // Simüle edilmiş bir bekleme süresi
        await Task.Delay(500, cancellationToken);

        // Test senaryosu 1: Eğer tutar 10000'den büyükse bilerek başarısız yapıyoruz ki compensation test edilebilsin.
        if (amount > 10000m)
        {
            return new PaymentResult(false, null, "Insufficient funds / Limit exceeded.");
        }

        // Test senaryosu 2: Rastgele hata oluşturma veya genel başarı (şu an hep başarılı)
        var transactionId = $"TXN-{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}";
        
        return new PaymentResult(true, transactionId, null);
    }
}
