using System.Text.Json;
using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Infrastructure.Database.Entities;

namespace ECommerce.API.Modules.Audit.Features.GetLogs;

public record LogDto(
    Guid Id,
    string? TraceId,
    string? UserId,
    DateTime Timestamp,
    string LogType,
    string Level,
    string? Source,
    string? Message,
    string? Payload,
    long? ElapsedMs,
    string? Exception,
    string? TableName,
    string? Action,
    string? KeyValues,
    string? OldValues,
    string? NewValues,
    string? EntityId,
    string? EntityName,
    string? UserEmail,
    string? UserName
);

public record GetLogsResponse(int TotalCount, List<LogDto> Items);

public class GetLogsQueryHandler : IQueryHandler<GetLogsQuery, GetLogsResponse>
{
    private readonly AppDbContext _dbContext;

    public GetLogsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetLogsResponse> HandleAsync(GetLogsQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<SystemLog> query = _dbContext.SystemLogs.AsNoTracking();

        // Tarih Filtresi
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value;
            DateTime startUtc = start.Kind switch
            {
                DateTimeKind.Utc => start,
                DateTimeKind.Local => start.ToUniversalTime(),
                _ => DateTime.SpecifyKind(start.Date, DateTimeKind.Utc)
            };
            query = query.Where(l => l.Timestamp >= startUtc);
        }

        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value;
            DateTime endUtc;
            if (end.Kind == DateTimeKind.Utc)
            {
                endUtc = end.TimeOfDay == TimeSpan.Zero ? end.Date.AddDays(1).AddTicks(-1) : end;
            }
            else if (end.Kind == DateTimeKind.Local)
            {
                endUtc = (end.TimeOfDay == TimeSpan.Zero ? end.Date.AddDays(1).AddTicks(-1) : end).ToUniversalTime();
            }
            else
            {
                endUtc = DateTime.SpecifyKind(end.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            }
            query = query.Where(l => l.Timestamp <= endUtc);
        }

        // Tür ve Seviye
        if (!string.IsNullOrWhiteSpace(request.LogType) && request.LogType.ToLower() != "all")
        {
            query = query.Where(l => l.LogType == request.LogType);
        }

        if (!string.IsNullOrWhiteSpace(request.Level) && request.Level.ToLower() != "all")
        {
            query = query.Where(l => l.Level == request.Level);
        }

        // Arama (Arama kutusuna ürün id'si veya adı girildiğinde eşleşmesi için)
        var effectiveSearch = !string.IsNullOrWhiteSpace(request.SearchText) ? request.SearchText : request.Search;
        if (!string.IsNullOrWhiteSpace(effectiveSearch))
        {
            var pattern = $"%{effectiveSearch.Trim()}%";
            query = query.Where(l => 
                (l.Message != null && EF.Functions.ILike(l.Message, pattern)) ||
                (l.TableName != null && EF.Functions.ILike(l.TableName, pattern)) ||
                (l.Action != null && EF.Functions.ILike(l.Action, pattern)) ||
                (l.KeyValues != null && EF.Functions.ILike(l.KeyValues, pattern)) ||
                (l.NewValues != null && EF.Functions.ILike(l.NewValues, pattern)) ||
                (l.OldValues != null && EF.Functions.ILike(l.OldValues, pattern)) ||
                (l.Payload != null && EF.Functions.ILike(l.Payload, pattern)) ||
                (l.TraceId != null && EF.Functions.ILike(l.TraceId, pattern)) ||
                (l.UserId != null && EF.Functions.ILike(l.UserId, pattern))
            );
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var dbLogs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Frontend'e giderken EntityId ve EntityName'i JSON'dan ayıklıyoruz
        // Ayrıca UserId (GUID) alanını gerçek kullanıcının Email/AdSoyad bilgileriyle zenginleştiriyoruz (Enrichment)
        
        var userGuids = dbLogs
            .Where(l => !string.IsNullOrWhiteSpace(l.UserId) && l.UserId != "System" && Guid.TryParse(l.UserId, out _))
            .Select(l => Guid.Parse(l.UserId!))
            .Distinct()
            .ToList();

        var userMap = new Dictionary<string, (string Email, string Name)>();
        if (userGuids.Any())
        {
            userMap = await _dbContext.Users
                .AsNoTracking()
                .Where(u => userGuids.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id.ToString(), 
                    u => (u.Email, $"{u.FirstName} {u.LastName}".Trim()), 
                    cancellationToken
                );
        }

        var dtos = dbLogs.Select(l => 
        {
            var auditInfo = ExtractAuditInfo(l.KeyValues, l.NewValues, l.OldValues, l.Payload, l.TableName);
            
            string? userEmail = null;
            string? userName = null;

            if (l.UserId == "System")
            {
                userEmail = "Sistem";
                userName = "Sistem";
            }
            else if (l.UserId != null && userMap.TryGetValue(l.UserId, out var uInfo))
            {
                userEmail = uInfo.Email;
                userName = !string.IsNullOrWhiteSpace(uInfo.Name) ? uInfo.Name : uInfo.Email;
            }
            else
            {
                userEmail = l.UserId; // Bulunamazsa guid'i fallback yap
                userName = l.UserId;
            }

            return new LogDto(
                l.Id, l.TraceId, l.UserId, l.Timestamp, l.LogType, l.Level,
                l.Source, l.Message, l.Payload, l.ElapsedMs, l.Exception,
                l.TableName, l.Action, l.KeyValues, l.OldValues, l.NewValues,
                auditInfo.EntityId, auditInfo.EntityName,
                userEmail, userName
            );
        }).ToList();

        return new GetLogsResponse(totalCount, dtos);
    }

    private static (string? EntityId, string? EntityName) ExtractAuditInfo(
        string? keyValuesJson, 
        string? newValuesJson, 
        string? oldValuesJson, 
        string? payloadJson, 
        string? tableName)
    {
        string? entityId = null;
        string? entityName = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(keyValuesJson) && keyValuesJson != "{}")
            {
                using var doc = JsonDocument.Parse(keyValuesJson);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    entityId = prop.Value.ToString();
                    break;
                }
            }

            // Arama sırası: Önce yeni, sonra eski, sonra tam snapshot (payload)
            string?[] jsonsToInspect = { newValuesJson, oldValuesJson, payloadJson };

            foreach (var json in jsonsToInspect)
            {
                if (string.IsNullOrWhiteSpace(json) || json == "{}") continue;

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Name", out var nameProp) && nameProp.ValueKind != JsonValueKind.Null)
                    entityName = nameProp.GetString();
                else if (doc.RootElement.TryGetProperty("Title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null)
                    entityName = titleProp.GetString();
                else if (doc.RootElement.TryGetProperty("Email", out var emailProp) && emailProp.ValueKind != JsonValueKind.Null)
                    entityName = emailProp.GetString();
                else if (doc.RootElement.TryGetProperty("FirstName", out var fnProp) && fnProp.ValueKind != JsonValueKind.Null && 
                         doc.RootElement.TryGetProperty("LastName", out var lnProp) && lnProp.ValueKind != JsonValueKind.Null)
                    entityName = $"{fnProp.GetString()} {lnProp.GetString()}";
                else if (doc.RootElement.TryGetProperty("OrderNumber", out var orderNoProp) && orderNoProp.ValueKind != JsonValueKind.Null)
                    entityName = orderNoProp.GetString();
                else if (doc.RootElement.TryGetProperty("Permission", out var permProp) && permProp.ValueKind != JsonValueKind.Null)
                    entityName = permProp.GetString();

                if (!string.IsNullOrWhiteSpace(entityName))
                    break; // Bulduysak aramayı bırak
            }
        }
        catch
        {
            // Parse hatasında yoksay
        }

        // Akıllı Fallback (Hiçbirinde isim yoksa "İsimsiz Kayıt" yerine id ile tabloyu birleştir)
        if (string.IsNullOrWhiteSpace(entityName) && !string.IsNullOrWhiteSpace(tableName))
        {
            var shortId = !string.IsNullOrWhiteSpace(entityId) && entityId.Length > 8 
                ? entityId[..8] 
                : entityId;
            entityName = $"[{tableName}] {shortId}";
        }

        return (entityId, entityName);
    }
}
