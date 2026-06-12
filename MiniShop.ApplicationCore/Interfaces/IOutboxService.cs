using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IOutboxService
{
    Task SaveAsync(IIntegrationEvent integrationEvent);
    Task PublishedPendingAsync();
}