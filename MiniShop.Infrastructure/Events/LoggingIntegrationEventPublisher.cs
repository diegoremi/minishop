using Microsoft.Extensions.Logging;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Events;

public class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

    public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }
    
    public Task PublishAsync(IIntegrationEvent integrationEvent)
    {
        _logger.LogInformation(
            "Integration event published: {EventName} {@Event}",
            integrationEvent.GetType().Name,
            integrationEvent
            );
        
        return Task.CompletedTask;
    }
}