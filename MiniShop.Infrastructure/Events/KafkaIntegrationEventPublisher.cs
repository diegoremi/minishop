using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.Infrastructure.Events;

public sealed class KafkaIntegrationEventPublisher: IIntegrationEventPublisher, IDisposable
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaIntegrationEventPublisher> _logger;
    private readonly IProducer<string, string> _producer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public KafkaIntegrationEventPublisher(
        IOptions<KafkaOptions> options,
        ILogger<KafkaIntegrationEventPublisher> logger )
    {
        _options = options.Value;
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = "minishop-webapi"
        };
        
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }
    
    public async Task PublishAsync(IIntegrationEvent integrationEvent)
    {
        var topic = GetTopicName(integrationEvent);
        var key = GetKey(integrationEvent);

        var value = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            JsonOptions
        );
        
        var message = new Message<string, string>
        {
            Key = key,
            Value = value
        };

        var result = await _producer.ProduceAsync(topic, message);

        _logger.LogInformation(
            "Kafka integration event published. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}",
            result.Topic,
            result.Partition,
            result.Offset,
            key);
    }

    private string GetTopicName(IIntegrationEvent integrationEvent)
    {
        return integrationEvent switch
        {
            OrderPlacedIntegrationEvent => _options.OrderPlacedTopic,
            OrderPaidIntegrationEvent => _options.OrderPaidTopic,
            _ => throw new InvalidOperationException(
                $"No Kafka topic configured for integration event type {integrationEvent.GetType().Name}")
        };
    }

    private static string GetKey(IIntegrationEvent integrationEvent)
    {
        return integrationEvent switch
        {
            OrderPlacedIntegrationEvent orderPlaced => orderPlaced.OrderId.ToString(),
            OrderPaidIntegrationEvent orderPaid => orderPaid.OrderId.ToString(),
            _ => integrationEvent.EventId.ToString()
        };
    }
    
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}