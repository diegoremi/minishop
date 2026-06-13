using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.IntegrationTests.Fakes;

public sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
{
    public List<IIntegrationEvent> PublishedEvents { get; } = [];

    public Task PublishAsync(IIntegrationEvent integrationEvent)
    {
        PublishedEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}