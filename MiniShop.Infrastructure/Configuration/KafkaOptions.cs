using System.ComponentModel.DataAnnotations;

namespace MiniShop.Infrastructure.Configuration;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    [Required] 
    public string BootstrapServers { get; init; } = string.Empty;
    
    [Required]
    public string OrderPlacedTopic { get; init; } = string.Empty;
    
    [Required]
    public string PaymentCompletedTopic { get; init; } = string.Empty;
    
    [Required]
    public string OrderPaidTopic { get; init; } = string.Empty;

    [Required]
    public string PaymentWorkerConsumerGroup { get; init; } = string.Empty;

    [Required]
    public string WebApiConsumerGroup { get; init; } = string.Empty;

}