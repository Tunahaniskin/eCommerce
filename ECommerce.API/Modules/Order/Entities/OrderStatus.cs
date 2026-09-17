namespace ECommerce.API.Modules.Order.Entities;

public enum OrderStatus
{
    Pending = 1,          // 1. Sipariş geldi, işlem sırasına alındı.
    StockReserved = 2,    // 2. Stok başarıyla rezerve edildi, ödeme bekleniyor.
    Paid = 3,             // 3. Ödeme alındı, sipariş tamamlandı (Başarılı Sonuç).
    Cancelled = 4         // 4. Stok yetersizliği veya Ödeme hatası nedeniyle iptal (Başarısız Sonuç).
}