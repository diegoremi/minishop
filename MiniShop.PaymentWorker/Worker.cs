using System.Text.Json;
using Confluent.Kafka;
using DefaultNamespace;
using Microsoft.Extensions.Options;
using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.PaymentWorker;

public sealed class Worker : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly ILogger<Worker> _logger;
    private readonly IPaymentCompletedEventPublisher _paymentCompletedEventPublisher;

    public Worker(IOptions<KafkaOptions> options,
        ILogger<Worker> logger,
        IPaymentCompletedEventPublisher paymentCompletedEventPublisher)
    {
        _options = options.Value;
        _logger = logger;
        _paymentCompletedEventPublisher = paymentCompletedEventPublisher;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }

    private async void Consume(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(_options.OrderPlacedTopic);

        _logger.LogInformation(
            "PaymentWorker started. Listening topic: {Topic}",
            _options.OrderPlacedTopic
        );

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    var orderPlacedEvent =
                        JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(
                            result.Message.Value,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });


                    if (orderPlacedEvent is null)
                    {
                        _logger.LogWarning("Received null or invalid OrderPlacedIntegrationEvent.");
                        consumer.Commit(result);
                        continue;   
                    }
                    
                    _logger.LogInformation(
                        "Order placed event consumed. OrderId: {OrderId}, Customer: {CustomerName}, Total: {Total}",
                        orderPlacedEvent.OrderId,
                        orderPlacedEvent.CustomerName,
                        orderPlacedEvent.Total
                        );
                    
                    _logger.LogInformation(
                        "Simulating payment process for OrderId {OrderId}...",
                        orderPlacedEvent.OrderId);
                    
                    Thread.Sleep(1000);
                    
                    var paymentCompletedEvent = new PaymentCompletedIntegrationEvent(
                        EventId: Guid.NewGuid(),
                        OrderId: orderPlacedEvent.OrderId,
                        CustomerName: orderPlacedEvent.CustomerName,
                        Amount: orderPlacedEvent.Total,
                        OccurredOnUtc: DateTime.UtcNow);

                    await _paymentCompletedEventPublisher.PublishAsync(
                        paymentCompletedEvent,
                        stoppingToken);
                    
                    consumer.Commit(result);
                    
                    _logger.LogInformation(
                        "Payment simulation finished for OrderId {OrderId}.",
                        orderPlacedEvent.OrderId);
                    
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error.");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing message.");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}