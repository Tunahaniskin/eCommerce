namespace ECommerce.API.Shared.Contracts;

// Sipariş Oluştu (Katalog modülü bunu dinleyip stoğu rezerve etmeye çalışacak)
public record OrderCreatedEvent(Guid OrderId, List<OrderItemDto> Items);
public record OrderItemDto(Guid ProductId, int Quantity);

// Bu bir Command (Komut) mesajıdır, Event (Olay) değildir.
public record CreateOrderMessage(Guid OrderId, List<OrderItemDto> Items);

// Katalog rezerve etmeyi başarırsa bunu fırlatacak (Ödeme modülü dinleyecek)
public record StockReservedEvent(Guid OrderId, decimal TotalAmount);

// Katalog stok bulamazsa bunu fırlatacak (Order modülü dinleyip siparişi Cancelled yapacak)
public record StockReservationFailedEvent(Guid OrderId, string Reason);

// Ödeme başarılı olursa (Order ve Catalog modülleri dinleyecek)
public record PaymentSucceededEvent(Guid OrderId);

// Ödeme patlarsa (Order ve Catalog modülleri dinleyecek, rollback yapacaklar)
public record PaymentFailedEvent(Guid OrderId, string Reason);

public record ProductDeletedEvent(Guid ProductId);
