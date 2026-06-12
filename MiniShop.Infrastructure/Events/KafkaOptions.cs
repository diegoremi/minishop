namespace MiniShop.Infrastructure.Events;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string OrderPlacedTopic { get; set; } = "minishop.order-placed.v1";
    public string OrderPaidTopic { get; set; } = "minishop.order-paid.v1";
}