using ECommerce.API.Infrastructure.Database.Entities; // 1. EKLENEN USING
using ECommerce.API.Modules.Catalog.Entities;
using ECommerce.API.Modules.Order.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Ortak Altyapı Tabloları
    public DbSet<AuditLog> AuditLogs { get; set; } // 2. EKLENEN DBSET

    // Catalog Modülü Tabloları
    public DbSet<Product> Products { get; set; }

    // Order Modülü Tabloları
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Modüler izolasyon için tabloları şemalara (schemas) ayırıyoruz.
        
        // Global Query Filter: Sadece silinmemiş ürünleri getir
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        
        // Catalog Modülü
        modelBuilder.Entity<Product>().ToTable("Products", "catalog");

        // Order Modülü
        modelBuilder.Entity<Order>().ToTable("Orders", "order");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "order");

        // 3. EKLENEN ŞEMA TANIMI: AuditLogs tablosunu 'audit' şemasına taşıyoruz
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs", "audit");
    }
}