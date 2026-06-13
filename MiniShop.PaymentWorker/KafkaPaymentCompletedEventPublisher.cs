using System.Text.Json;
using Confluent.Kafka;
using DefaultNamespace;
using Microsoft.Extensions.Options;
using MiniShop.ApplicationCore.IntegrationEvents;

namespace MiniShop.PaymentWorker;

public class KafkaPaymentCompletedEventPublisher : IPaymentCompletedEventPublisher, IDisposable
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaPaymentCompletedEventPublisher> _logger;
    private readonly IProducer<string, string> _producer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public KafkaPaymentCompletedEventPublisher(
        IOptions<KafkaOptions> options,
        ILogger<KafkaPaymentCompletedEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = "minishop-payment-worker"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    
    public async Task PublishAsync(
        PaymentCompletedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken)
    {
        var value = JsonSerializer.Serialize(
            integrationEvent,
            JsonOptions
        );

        var message = new Message<string, string>
        {
            Key = integrationEvent.OrderId.ToString(),
            Value = value
        };

        var result = await _producer.ProduceAsync(
            _options.PaymentCompletedTopic,
            message,
            cancellationToken
        );
        
        _logger.LogInformation(
            "PaymentCompleted event published. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, OrderId: {OrderId}",
            result.Topic,
            result.Partition,
            result.Offset,
            integrationEvent.OrderId);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}