using ECommerce.API.Infrastructure.Database.Entities; // 1. EKLENEN USING
using ECommerce.API.Modules.Product.Entities;
using ECommerce.API.Modules.Order.Entities;
using ECommerce.API.Modules.Auth.Entities;
using ECommerce.API.Modules.Payment.Entities;
using ECommerce.API.Modules.Finance.Entities;
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
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserTemporaryPermission> UserTemporaryPermissions { get; set; }

    // Order Modülü Tabloları
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Payment Modülü Tabloları
    public DbSet<Payment> Payments { get; set; }

    // Finance Modülü Tabloları
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<DepositTicket> DepositTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Modüler izolasyon için tabloları şemalara (schemas) ayırıyoruz.
        
        // Global Query Filter: Sadece silinmemiş ürünleri getir
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        
        // Product Modülü
        modelBuilder.Entity<Category>().ToTable("Categories", "product");
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        
        modelBuilder.Entity<Product>().ToTable("Products", "product");
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<ProductImage>().ToTable("ProductImages", "product");
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // Finance Modülü
        modelBuilder.Entity<Wallet>().ToTable("Wallets", "finance");
        modelBuilder.Entity<Wallet>().Property(w => w.Balance).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Wallet>().HasIndex(w => w.UserId).IsUnique(); // Bir kullanıcının bir cüzdanı olur

        modelBuilder.Entity<WalletTransaction>().ToTable("WalletTransactions", "finance");
        modelBuilder.Entity<WalletTransaction>().Property(wt => wt.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<DepositTicket>().ToTable("DepositTickets", "finance");
        modelBuilder.Entity<DepositTicket>().Property(dt => dt.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<DepositTicket>()
            .Property(dt => dt.Status)
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

        modelBuilder.Entity<Address>().ToTable("Addresses", "auth");

        modelBuilder.Entity<Role>().ToTable("Roles", "auth");
        
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions", "auth");
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.Permission });

        modelBuilder.Entity<UserTemporaryPermission>().ToTable("UserTemporaryPermissions", "auth");

        // Global Query Filter (Sadece silinmemiş kullanıcıları getir)
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    }
}