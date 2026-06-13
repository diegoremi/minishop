using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.PaymentWorker;

public interface IPaymentCompletedEventPublisher
{
    Task PublishAsync(
        PaymentCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}