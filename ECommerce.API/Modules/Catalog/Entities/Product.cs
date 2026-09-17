namespace ECommerce.API.Modules.Catalog.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; } 
    public int ReservedStock { get; private set; } 
    public bool IsDeleted { get; private set; }

    private Product() { }

    public Product(string name, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Stock = stock;
        ReservedStock = 0;
    }

    // 1. ADIM: Stok yeterliyse rezerve et
    public bool ReserveStock(int quantity)
    {
        // Kullanılabilir stok = Toplam Stok - Rezerve Edilmiş Stok
        var availableStock = Stock - ReservedStock;
        
        if (availableStock >= quantity)
        {
            ReservedStock += quantity;
            return true;
        }
        return false;
    }

    // 2. ADIM (BAŞARILI): Ödeme alındı, rezerve stoğu tamamen sistemden düş
    public void CommitStock(int quantity)
    {
        Stock -= quantity;
        ReservedStock -= quantity;
    }

    // 2. ADIM (BAŞARISIZ): Ödeme patladı, ayrılan stoğu serbest bırak (Rollback)
    public void RollbackStock(int quantity)
    {
        ReservedStock -= quantity;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}