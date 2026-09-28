namespace ECommerce.API.Infrastructure.Database.Entities;

public class ProcessedMessage
{
    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedMessage() { } // EF Core için

    public ProcessedMessage(Guid messageId, string consumerName)
    {
        MessageId = messageId;
        ConsumerName = consumerName;
        ProcessedAt = DateTime.UtcNow;
    }
}
