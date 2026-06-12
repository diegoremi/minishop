using MiniShop.ApplicationCore.Events;

namespace MiniShop.ApplicationCore.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEventsAsync(IEnumerable<IDomainEvent> domainEvents);
}