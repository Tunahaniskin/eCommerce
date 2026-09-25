using ECommerce.API.Infrastructure.Database.Entities; // 1. EKLENEN USING
using ECommerce.API.Modules.Product.Entities;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Modules.Auth.Entities;
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

    public DbSet<User> Users { get; set; }

    // Order Modülü Tabloları
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Modüler izolasyon için tabloları şemalara (schemas) ayırıyoruz.
        
        // Global Query Filter: Sadece silinmemiş ürünleri getir
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        
        // Product Modülü
        modelBuilder.Entity<Product>().ToTable("Products", "product");

        // Order Modülü
        modelBuilder.Entity<Order>().ToTable("Orders", "order");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "order");

        // 3. EKLENEN ŞEMA TANIMI: AuditLogs tablosunu 'audit' şemasına taşıyoruz
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs", "audit");

        // Auth Modülü
        modelBuilder.Entity<User>().ToTable("Users", "auth");
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Permissions listesini PostgreSQL text array olarak sakla
        modelBuilder.Entity<User>()
            .Property(u => u.Permissions)
            .HasColumnType("text[]");

        // Global Query Filter (Sadece silinmemiş kullanıcıları getir)
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    }
}