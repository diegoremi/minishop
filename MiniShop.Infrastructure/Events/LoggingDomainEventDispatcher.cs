using Microsoft.Extensions.Logging;
using MiniShop.ApplicationCore.Events;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Events;

public class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;
    private readonly IOutboxService _outboxService;

    public LoggingDomainEventDispatcher(
        ILogger<LoggingDomainEventDispatcher> logger,
        IOutboxService outboxService)
    {
        _logger = logger;
        _outboxService = outboxService;
    }

    public async Task DispatchAndClearEventsAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation(
                "Domain event dispatched: {EventName} at {OccurredOnUtc}",
                domainEvent.GetType().Name,
                domainEvent.OccurredOnUtc
            );

            switch (domainEvent)
            {
                case OrderPlacedEvent orderPlaced:
                    await _outboxService.SaveAsync(
                        new OrderPlacedIntegrationEvent(
                            Guid.NewGuid(),
                            orderPlaced.OrderId,
                            orderPlaced.CustomerName,
                            orderPlaced.Total,
                            DateTime.UtcNow
                        )
                    );
                    break;

                case OrderPaidEvent orderPaid:
                    await _outboxService.SaveAsync(
                        new OrderPaidIntegrationEvent(
                            Guid.NewGuid(),
                            orderPaid.OrderId,
                            orderPaid.CustomerName,
                            orderPaid.Total,
                            DateTime.UtcNow
                        )
                    );
                    break;
            }
        }
    }
}