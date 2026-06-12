namespace MiniShop.ApplicationCore.Events;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}