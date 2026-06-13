using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IInboxService
{
    Task<bool> TryRegisterAsync(
        IIntegrationEvent integrationEvent,
        string consumer,
        CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(
        Guid eventId,
        string consumer,
        CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(
        Guid eventId,
        string consumer,
        string error,
        CancellationToken cancellationToken = default);
}