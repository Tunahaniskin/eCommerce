namespace ECommerce.API.Modules.Order.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    // YENİ EKLENEN ALAN
    public OrderStatus Status { get; private set; } 
    
    public List<OrderItem> Items { get; private set; } = new();

    private Order() { }

    public Order(Guid id, List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Sipariş en az bir ürün içermelidir.", nameof(items));

        Id = id; // ID artık veritabanı veya kendi içinde değil, Endpoint'ten geliyor
        CreatedAt = DateTime.UtcNow;
        Items = items;
        TotalAmount = items.Sum(x => x.UnitPrice * x.Quantity); // Fiyatlar Consumer'da gerçek değerleriyle set edilecek
        Status = OrderStatus.Pending; 
    }

    public void MarkAsStockReserved()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Yalnızca 'Pending' durumundaki siparişlerin stoğu rezerve edilebilir.");
        
        Status = OrderStatus.StockReserved;
    }

    public void MarkAsPaid()
    {
        // Pending durumundan da geçilebilir veya StockReserved şart koşulabilir. Order modülü StockReservedEvent'i dinlemediği için Pending'den Paid'e geçiş olabilir.
        if (Status != OrderStatus.StockReserved && Status != OrderStatus.Pending)
            throw new InvalidOperationException("Sipariş ödenemez. (Sipariş iptal edilmiş veya zaten ödenmiş.)");
        
        Status = OrderStatus.Paid;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Ödenmiş bir sipariş doğrudan iptal edilemez (iade süreci gerektirir).");
            
        if (Status == OrderStatus.Cancelled)
            return; // Zaten iptal edilmiş
            
        Status = OrderStatus.Cancelled;
        // Not: 'reason' değişkenini loglamak veya veritabanında tutmak için yeni bir alan eklenebilir.
    }
}


public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (unitPrice <= 0)
            throw new ArgumentException("Birim fiyat sıfırdan büyük olmalıdır.", nameof(unitPrice));

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}