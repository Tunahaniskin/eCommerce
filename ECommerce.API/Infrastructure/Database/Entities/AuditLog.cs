namespace ECommerce.API.Infrastructure.Database.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public string TableName { get; private set; }
    public string Action { get; private set; }
    public string KeyValues { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? UserId { get; private set; }

    private AuditLog() 
    { 
        TableName = default!;
        Action = default!;
        KeyValues = default!;
    }

    public AuditLog(string tableName, string action, string keyValues, string? oldValues, string? newValues, string? userId)
    {
        Id = Guid.NewGuid();
        TableName = tableName;
        Action = action;
        KeyValues = keyValues;
        OldValues = oldValues;
        NewValues = newValues;
        Timestamp = DateTime.UtcNow;
        UserId = userId;
    }
}