namespace MiniShop.Infrastructure.Events;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string OrderPlacedTopic { get; set; } = "minishop.order-placed.v1";
    public string OrderPaidTopic { get; set; } = "minishop.order-paid.v1";
    
    public string PaymentCompletedTopic { get; set; } = "minishop.payment-completed.v1";
    public string PaymentCompletedDeadLetterTopic { get; set; } = "minishop.payment-completed.dlq.v1";
    public string PaymentCompletedConsumerGroup { get; set; } = "minishop-orders";

    public int PaymentCompletedMaxProcessingRetries { get; set; } = 3;
}