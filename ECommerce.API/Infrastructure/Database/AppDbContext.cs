using ECommerce.API.Infrastructure.Database.Entities; // 1. EKLENEN USING
using ECommerce.API.Modules.Product.Entities;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Modules.Auth.Entities;
using ECommerce.API.Modules.Payment.Entities;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.API.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Ortak Altyapı Tabloları
    public DbSet<SystemLog> SystemLogs { get; set; } // 2. EKLENEN DBSET
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    // Catalog Modülü Tabloları
    public DbSet<Product> Products { get; set; }

    public DbSet<User> Users { get; set; }

    // Order Modülü Tabloları
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Payment Modülü Tabloları
    public DbSet<Payment> Payments { get; set; }

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
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30);
        
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "order");

        // Payment Modülü
        modelBuilder.Entity<Payment>().ToTable("Payments", "payment");
        modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        // 3. EKLENEN ŞEMA TANIMI: SystemLogs tablosunu 'audit' şemasına taşıyoruz
        modelBuilder.Entity<SystemLog>().ToTable("SystemLogs", "audit");

        // Idempotency Tablosu
        modelBuilder.Entity<ProcessedMessage>().ToTable("ProcessedMessages", "audit");
        modelBuilder.Entity<ProcessedMessage>().HasKey(p => new { p.MessageId, p.ConsumerName });

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