using System.Security.Claims;
using System.Text.Json;
using ECommerce.API.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ECommerce.API.Infrastructure.Database.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<SystemLog>();

        // HTTP Context'ten TraceId ve UserId al
        var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

        // ChangeTracker üzerinden değişen tüm entity'leri yakala
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            // Değişmeyenleri, izlenmeyenleri ve SystemLog'un kendisini (sonsuz döngüyü engellemek için) atla
            if (entry.Entity is SystemLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            
            var keyValues = new Dictionary<string, object?>();
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            // Sütunları tek tek dönerek eski ve yeni değerleri ayıkla
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    keyValues[propertyName] = property.CurrentValue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            // Toplanan verileri JSON formatına dönüştürüp listeye ekle
            auditEntries.Add(SystemLog.CreateAudit(
                traceId,
                userId,
                tableName,
                action,
                JsonSerializer.Serialize(keyValues),
                oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues)
            ));
        }

        // Yakalanan audit log kayıtlarını veritabanına eklenmek üzere Context'e dahil et
        if (auditEntries.Any())
        {
            dbContext.Set<SystemLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}