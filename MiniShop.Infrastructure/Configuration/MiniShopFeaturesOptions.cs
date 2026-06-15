namespace MiniShop.Infrastructure.Configuration;

public class MiniShopFeaturesOptions
{
    public bool UseRedis { get; init; } = true;

    public bool UseKafka { get; init; } = true;

    public bool RunOutboxPublisher { get; init; } = true;

    public bool RunPaymentCompletedConsumer { get; init; } = true;

    public bool ApplyMigrationsOnStartup { get; init; } = true;
    
    public bool CreateDatabaseOnStartup { get; init; } = true;
}