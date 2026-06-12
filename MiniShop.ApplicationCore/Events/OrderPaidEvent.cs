namespace MiniShop.ApplicationCore.Events;

public record OrderPaidEvent(
    int OrderId,
    string CustomerName,
    decimal Total,
    DateTime OccurredOnUtc
) : IDomainEvent;