namespace MiniShop.Infrastructure.Events;

public interface IDeadLetterPublisher
{
    Task PublishAsync(
        string topic,
        DeadLetterMessage message,
        CancellationToken cancellationToken = default);
}