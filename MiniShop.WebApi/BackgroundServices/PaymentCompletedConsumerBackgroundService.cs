using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Events;

namespace MiniShop.WebApi.BackgroundServices;

public sealed class PaymentCompletedConsumerBackgroundService : BackgroundService
{
    private const string ConsumerName = "minishop-orders-payment-completed-consumer";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly IDeadLetterPublisher _deadLetterPublisher;
    private readonly ILogger<PaymentCompletedConsumerBackgroundService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymentCompletedConsumerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        IDeadLetterPublisher deadLetterPublisher,
        ILogger<PaymentCompletedConsumerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _deadLetterPublisher = deadLetterPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.PaymentCompletedConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(_options.PaymentCompletedTopic);

        _logger.LogInformation(
            "PaymentCompleted consumer started. Listening topic: {Topic}",
            _options.PaymentCompletedTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    var paymentCompletedEvent = TryDeserializePaymentCompletedEvent(
                        result,
                        out var deserializeError);

                    if (paymentCompletedEvent is null)
                    {
                        await PublishInvalidPayloadToDeadLetterAsync(
                            result,
                            deserializeError ?? "Invalid PaymentCompletedIntegrationEvent payload.",
                            stoppingToken);

                        consumer.Commit(result);
                        continue;
                    }

                    await ProcessPaymentCompletedWithRetryAsync(
                        result,
                        paymentCompletedEvent,
                        stoppingToken);

                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka consume error while reading PaymentCompleted event.");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error in PaymentCompleted consumer loop. Message was not committed unless it was handled.");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private PaymentCompletedIntegrationEvent? TryDeserializePaymentCompletedEvent(
        ConsumeResult<string, string> result,
        out string? error)
    {
        try
        {
            error = null;

            return JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(
                result.Message.Value,
                JsonOptions);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    private async Task ProcessPaymentCompletedWithRetryAsync(
        ConsumeResult<string, string> result,
        PaymentCompletedIntegrationEvent paymentCompletedEvent,
        CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _options.PaymentCompletedMaxProcessingRetries);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ProcessPaymentCompletedAsync(
                    paymentCompletedEvent,
                    cancellationToken);

                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                _logger.LogWarning(
                    ex,
                    "Error processing PaymentCompleted event. Attempt {Attempt}/{MaxAttempts}. EventId: {EventId}, OrderId: {OrderId}",
                    attempt,
                    maxAttempts,
                    paymentCompletedEvent.EventId,
                    paymentCompletedEvent.OrderId);

                if (attempt < maxAttempts)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(attempt),
                        cancellationToken);
                }
            }
        }

        var error = lastException?.ToString() ?? "Unknown processing error.";

        await PublishPaymentCompletedToDeadLetterAsync(
            result,
            paymentCompletedEvent,
            error,
            maxAttempts,
            cancellationToken);

        using var scope = _scopeFactory.CreateScope();

        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();

        await inboxService.MarkAsDeadLetteredAsync(
            paymentCompletedEvent.EventId,
            ConsumerName,
            error,
            cancellationToken);

        _logger.LogWarning(
            "PaymentCompleted event moved to DLQ after {Attempts} attempts. EventId: {EventId}, OrderId: {OrderId}",
            maxAttempts,
            paymentCompletedEvent.EventId,
            paymentCompletedEvent.OrderId);
    }

    private async Task ProcessPaymentCompletedAsync(
        PaymentCompletedIntegrationEvent paymentCompletedEvent,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        var shouldProcess = await inboxService.TryRegisterAsync(
            paymentCompletedEvent,
            ConsumerName,
            cancellationToken);

        if (!shouldProcess)
        {
            _logger.LogInformation(
                "PaymentCompleted event was already processed or dead-lettered. EventId: {EventId}, OrderId: {OrderId}",
                paymentCompletedEvent.EventId,
                paymentCompletedEvent.OrderId);

            return;
        }

        try
        {
            var order = await orderService.GetOrderByIdAsync(
                paymentCompletedEvent.OrderId);

            if (order is null)
            {
                throw new InvalidOperationException(
                    $"Order was not found. OrderId: {paymentCompletedEvent.OrderId}");
            }

            if (order.Status == OrderStatus.Paid)
            {
                _logger.LogInformation(
                    "PaymentCompleted received but Order is already paid. OrderId: {OrderId}",
                    paymentCompletedEvent.OrderId);

                await inboxService.MarkAsProcessedAsync(
                    paymentCompletedEvent.EventId,
                    ConsumerName,
                    cancellationToken);

                return;
            }

            if (order.Status != OrderStatus.Placed)
            {
                throw new InvalidOperationException(
                    $"Order is not in Placed status. OrderId: {paymentCompletedEvent.OrderId}, CurrentStatus: {order.Status}");
            }

            await orderService.PayOrderAsync(paymentCompletedEvent.OrderId);

            await inboxService.MarkAsProcessedAsync(
                paymentCompletedEvent.EventId,
                ConsumerName,
                cancellationToken);

            _logger.LogInformation(
                "Order marked as Paid from PaymentCompleted event. EventId: {EventId}, OrderId: {OrderId}, Amount: {Amount}",
                paymentCompletedEvent.EventId,
                paymentCompletedEvent.OrderId,
                paymentCompletedEvent.Amount);
        }
        catch (Exception ex)
        {
            await inboxService.MarkAsFailedAsync(
                paymentCompletedEvent.EventId,
                ConsumerName,
                ex.Message,
                cancellationToken);

            throw;
        }
    }

    private async Task PublishPaymentCompletedToDeadLetterAsync(
        ConsumeResult<string, string> result,
        PaymentCompletedIntegrationEvent paymentCompletedEvent,
        string error,
        int attempts,
        CancellationToken cancellationToken)
    {
        var deadLetterMessage = new DeadLetterMessage(
            OriginalTopic: result.Topic,
            OriginalPartition: result.Partition.Value,
            OriginalOffset: result.Offset.Value,
            Key: result.Message.Key,
            Payload: result.Message.Value,
            Error: error,
            Consumer: ConsumerName,
            EventType: nameof(PaymentCompletedIntegrationEvent),
            Attempts: attempts,
            FailedOnUtc: DateTime.UtcNow);

        await _deadLetterPublisher.PublishAsync(
            _options.PaymentCompletedDeadLetterTopic,
            deadLetterMessage,
            cancellationToken);
    }

    private async Task PublishInvalidPayloadToDeadLetterAsync(
        ConsumeResult<string, string> result,
        string error,
        CancellationToken cancellationToken)
    {
        var deadLetterMessage = new DeadLetterMessage(
            OriginalTopic: result.Topic,
            OriginalPartition: result.Partition.Value,
            OriginalOffset: result.Offset.Value,
            Key: result.Message.Key,
            Payload: result.Message.Value,
            Error: error,
            Consumer: ConsumerName,
            EventType: "InvalidPayload",
            Attempts: 0,
            FailedOnUtc: DateTime.UtcNow);

        await _deadLetterPublisher.PublishAsync(
            _options.PaymentCompletedDeadLetterTopic,
            deadLetterMessage,
            cancellationToken);
    }
}