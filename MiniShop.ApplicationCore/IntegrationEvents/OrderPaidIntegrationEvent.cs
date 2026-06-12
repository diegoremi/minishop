namespace MiniShop.ApplicationCore.IntegrationEvents;

public record OrderPaidIntegrationEvent(
    Guid EventId,
    int OrderId,
    string CustomerName,
    decimal Total,
    DateTime OccurredOnUtc
) : IIntegrationEvent;