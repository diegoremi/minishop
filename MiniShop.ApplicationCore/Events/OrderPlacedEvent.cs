namespace MiniShop.ApplicationCore.Events;

public record OrderPlacedEvent(
    int OrderId,
    string CustomerName,
    decimal Total,
    DateTime OccurredOnUtc
) : IDomainEvent;