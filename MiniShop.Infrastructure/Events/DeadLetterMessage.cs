namespace MiniShop.Infrastructure.Events;

public sealed record DeadLetterMessage(
    string OriginalTopic,
    int OriginalPartition,
    long OriginalOffset,
    string? Key,
    string Payload,
    string Error,
    string Consumer,
    string EventType,
    int Attempts,
    DateTime FailedOnUtc
);