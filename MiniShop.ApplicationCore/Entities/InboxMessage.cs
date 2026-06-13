namespace MiniShop.ApplicationCore.Entities;

public sealed class InboxMessage
{
    private InboxMessage()
    {
        
    }

    public InboxMessage(
        Guid eventId,
        string type,
        string content,
        string consumer)
    {
        EventId = eventId;
        Type = string.IsNullOrWhiteSpace(type)
            ? throw new ArgumentException("Type is required.", nameof(type))
            : type;

        Content = string.IsNullOrWhiteSpace(content)
            ? throw new ArgumentException("Content is required.", nameof(content))
            : content;

        Consumer = string.IsNullOrWhiteSpace(consumer)
            ? throw new ArgumentException("Consumer is required.", nameof(consumer))
            : consumer;

        ReceivedOnUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Consumer { get; private set; } = string.Empty;

    public DateTime ReceivedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }

    public int ProcessingAttempts { get; private set; }
    public DateTime? DeadLetteredOnUtc { get; private set; }

    public string? Error { get; private set; }
    
    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }
    
    public void MarkAsFailed(string error)
    {
        ProcessingAttempts++;
        Error = error;
    }
    
    public void MarkAsDeadLettered(string error)
    {
        DeadLetteredOnUtc = DateTime.UtcNow;
        Error = error;
    }
    
}