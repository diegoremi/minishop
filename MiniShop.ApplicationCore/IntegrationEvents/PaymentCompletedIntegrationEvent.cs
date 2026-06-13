namespace MiniShop.ApplicationCore.IntegrationEvents;

public record PaymentCompletedIntegrationEvent(
    Guid EventId,
    int OrderId,
    string CustomerName,
    decimal Amount,
    DateTime OccurredOnUtc
) : IIntegrationEvent;