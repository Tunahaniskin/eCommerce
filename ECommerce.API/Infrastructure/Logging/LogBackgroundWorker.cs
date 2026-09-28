using System.Threading.Channels;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.API.Infrastructure.Logging;

public class LogBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LogBackgroundWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Kuyrukta log oldukça tek tek (veya batch halinde) alır ve veritabanına yazar.
        await foreach (var log in LogQueue.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.SystemLogs.Add(log);
                
                // Uygulama çok yoğunsa burada AddRange ve listeleme (batch) yapılabilir.
                // Şimdilik kuyruktan geldikçe sırayla kaydediyoruz.
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Fallback olarak en azından konsola basalım ki log kaybını bilelim.
                Console.WriteLine($"[CRITICAL] LogBackgroundWorker veritabanına log yazamadı! Hata: {ex.Message}");
            }
        }
    }
}
