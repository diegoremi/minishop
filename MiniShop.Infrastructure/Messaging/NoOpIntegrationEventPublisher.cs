using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Messaging;

public sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(IIntegrationEvent integrationEvent)
    {
        return Task.CompletedTask;
    }
}