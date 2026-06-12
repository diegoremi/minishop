namespace MiniShop.ApplicationCore.Entities;

public class OutboxMessage
{
    public int Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage()
    {
        
    }

    public OutboxMessage(string type, string content, DateTime occurredOnUtc)
    {
        if(string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Outbox message type is required.",  nameof(type));
        if(string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Outbox message content is required.", nameof(content));

        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }
    
    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }
    
    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}