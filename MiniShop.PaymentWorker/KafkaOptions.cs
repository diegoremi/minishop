namespace DefaultNamespace;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string OrderPlacedTopic { get; set; } = string.Empty;
    public string ConsumerGroup { get; set; } = string.Empty;
}