namespace ECommerce.API.Modules.Product.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; } 
    public int ReservedStock { get; private set; } 
    public bool IsDeleted { get; private set; }

    private Product() 
    { 
        Name = default!; 
    }

    public Product(string name, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Stock = stock;
        ReservedStock = 0;
    }

    // 1. ADIM: Stok yeterliyse rezerve et
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Rezerve edilecek miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        if (Stock - ReservedStock < quantity)
            throw new InvalidOperationException("Yetersiz stok. İstenen miktar mevcut rezerve edilebilir stoğu aşıyor.");

        ReservedStock += quantity;
    }

    // 2. ADIM (BAŞARILI): Ödeme alındı, rezerve stoğu tamamen sistemden düş
    public void CommitStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Sistemden düşülecek miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        if (ReservedStock < quantity)
            throw new InvalidOperationException("Rezerve edilenden daha fazla stok sistemden düşülemez.");

        Stock -= quantity;
        ReservedStock -= quantity;
    }

    // 2. ADIM (BAŞARISIZ): Ödeme patladı, ayrılan stoğu serbest bırak (Rollback)
    public void RollbackStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Serbest bırakılacak miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        if (ReservedStock < quantity)
            throw new InvalidOperationException("Rezerve edilenden daha fazla stok serbest bırakılamaz.");

        ReservedStock -= quantity;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Ürün zaten silinmiş durumda.");

        IsDeleted = true;
    }
}
