using ECommerce.API.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Veritabanında SuperAdmin var mı kontrol et. 
        // IgnoreQueryFilters kullanıyoruz çünkü daha önce oluşturulup silinmiş (soft-delete) bir SuperAdmin 
        // varsa sistemi tekrar aynı e-posta ile kayıt yapmaya çalışıp Unique Constraint hatasına düşürmemeliyiz.
        bool hasSuperAdmin = await dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Role == UserRole.SuperAdmin);

        if (hasSuperAdmin)
        {
            return;
        }

        // 2. Çevre değişkenlerinden SuperAdmin bilgilerini oku
        var adminEmail = configuration["INITIAL_ADMIN_EMAIL"];
        var adminPassword = configuration["INITIAL_ADMIN_PASSWORD"];

        // Tanımlanmamışsa hata fırlatma, sessizce geç. (Zero-trust güvenliği)
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine("[Güvenlik Uyarısı] INITIAL_ADMIN_EMAIL veya INITIAL_ADMIN_PASSWORD tanımlanmadığı için sistem ilk yöneticisiz başlatılıyor.");
            return;
        }

        // 3. SuperAdmin'i oluştur. (User entity constructor'ı Permissions = ["*"] atamasını kendi yapıyor)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
        var superAdmin = new User(adminEmail, passwordHash, UserRole.SuperAdmin);

        dbContext.Users.Add(superAdmin);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"[Sistem] İlk SuperAdmin hesabı başarıyla tanımlandı: {adminEmail}");
    }
}