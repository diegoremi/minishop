using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiniShop.Infrastructure.Events;

public sealed class KafkaDeadLetterPublisher : IDeadLetterPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaDeadLetterPublisher> _logger;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public KafkaDeadLetterPublisher(
        IOptions<KafkaEventOptions> options,
        ILogger<KafkaDeadLetterPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = "minishop-webapi-dlq"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    
    public async Task PublishAsync(
        string topic, 
        DeadLetterMessage message, 
        CancellationToken cancellationToken = default)
    {
        var value = JsonSerializer.Serialize(message, JsonOptions);

        var kafkaMessage = new Message<string, string>
        {
            Key = message.Key ?? message.OriginalOffset.ToString(),
            Value = value
        };

        var result = await _producer.ProduceAsync(
            topic,
            kafkaMessage,
            cancellationToken);

        _logger.LogWarning(
            "Message published to DLQ. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, OriginalTopic: {OriginalTopic}, OriginalOffset: {OriginalOffset}",
            result.Topic,
            result.Partition,
            result.Offset,
            message.OriginalTopic,
            message.OriginalOffset);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}