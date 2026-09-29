using ECommerce.API.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Infrastructure.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Bekleyen tüm migration'ları veritabanına otomatik uygula
        await dbContext.Database.MigrateAsync();

        // 1. Rolleri Kontrol Et ve Oluştur
        var superAdminRoleName = "SuperAdmin";
        var superAdminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == superAdminRoleName);

        if (superAdminRole == null)
        {
            superAdminRole = new Role(superAdminRoleName);
            dbContext.Roles.Add(superAdminRole);
            
            // SuperAdmin'e Wildcard (*) yetkisini ver
            dbContext.RolePermissions.Add(new RolePermission(superAdminRole.Id, Permissions.Wildcard));
            
            await dbContext.SaveChangesAsync();
        }

        // 2. Veritabanında SuperAdmin kullanıcısı var mı kontrol et. 
        bool hasSuperAdminUser = await dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.RoleId == superAdminRole.Id);

        if (hasSuperAdminUser)
        {
            return;
        }

        // 3. .env dosyasından SuperAdmin bilgilerini oku
        var adminEmail = configuration["INITIAL_SUPERADMIN_EMAIL"];
        var adminPassword = configuration["INITIAL_SUPERADMIN_PASSWORD"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine("[Güvenlik Uyarısı] INITIAL_SUPERADMIN_EMAIL veya INITIAL_SUPERADMIN_PASSWORD .env'de tanımlanmadığı için sistem ilk yöneticisiz başlatılıyor.");
            return;
        }

        // 4. SuperAdmin'i oluştur.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
        var superAdminUser = new User(adminEmail, passwordHash, superAdminRole.Id);

        dbContext.Users.Add(superAdminUser);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"[Sistem] İlk SuperAdmin hesabı (.env üzerinden) başarıyla tanımlandı: {adminEmail}");
    }
}