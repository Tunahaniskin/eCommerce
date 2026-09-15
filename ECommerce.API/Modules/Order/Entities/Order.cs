namespace ECommerce.API.Modules.Order.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    private Order() { }

    public Order(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Sipariş en az bir ürün içermelidir.", nameof(items));

        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Items = items;
        TotalAmount = items.Sum(x => x.UnitPrice * x.Quantity);
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