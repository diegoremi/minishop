using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent);
}