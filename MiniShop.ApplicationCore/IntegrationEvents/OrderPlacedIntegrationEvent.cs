namespace MiniShop.ApplicationCore.IntegrationEvents;

public record OrderPlacedIntegrationEvent(
    Guid EventId,
    int OrderId,
    string CustomerName,
    decimal Total,
    DateTime OccurredOnUtc
) : IIntegrationEvent;