namespace ECommerce.API.Modules.Catalog.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    // EF Core için parametresiz ctor
    private Product() { }

    public Product(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
        if (price <= 0)
            throw new ArgumentException("Fiyat sıfırdan büyük olmalıdır.", nameof(price));
        if (stock < 0)
            throw new ArgumentException("Stok negatif olamaz.", nameof(stock));

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Stock = stock;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Düşülecek miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (Stock - quantity < 0)
            throw new InvalidOperationException($"Yetersiz stok. Mevcut stok: {Stock}");

        Stock -= quantity;
    }
}