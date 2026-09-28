using System;

namespace ECommerce.API.Infrastructure.Database.Entities;

public class SystemLog
{
    public Guid Id { get; private set; }
    
    // Core Metadata
    public string? TraceId { get; private set; }
    public string? UserId { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    // Uygulama & Performans & Security Logları İçin
    public string LogType { get; private set; } // "Audit", "Application", "Security", "Performance"
    public string Level { get; private set; } // "Info", "Warning", "Error", "Critical", "Debug"
    public string? Source { get; private set; }
    public string? Message { get; private set; }
    public string? Payload { get; private set; }
    public long? ElapsedMs { get; private set; }
    public string? Exception { get; private set; }

    // Audit (Veri Değişiklik İzi) Logları İçin
    public string? TableName { get; private set; }
    public string? Action { get; private set; }
    public string? KeyValues { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    // EF Core Constructor
    private SystemLog() 
    { 
        LogType = default!;
        Level = default!;
    }

    // AuditLog Constructor'ı (Veritabanı değişikliği için)
    public static SystemLog CreateAudit(string? traceId, string? userId, string tableName, string action, string keyValues, string? oldValues, string? newValues)
    {
        return new SystemLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            TraceId = traceId,
            UserId = userId,
            LogType = "Audit",
            Level = "Info",
            TableName = tableName,
            Action = action,
            KeyValues = keyValues,
            OldValues = oldValues,
            NewValues = newValues
        };
    }

    // Application Log Constructor'ı (Akış logları için)
    public static SystemLog CreateAppLog(string? traceId, string? userId, string logType, string level, string? source, string message, string? payload = null, long? elapsedMs = null, string? exception = null)
    {
        return new SystemLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            TraceId = traceId,
            UserId = userId,
            LogType = logType,
            Level = level,
            Source = source,
            Message = message,
            Payload = payload,
            ElapsedMs = elapsedMs,
            Exception = exception
        };
    }
}
