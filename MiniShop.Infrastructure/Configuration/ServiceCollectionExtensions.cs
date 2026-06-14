using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiniShop.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniShopConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<InboxOptions>()
            .Bind(configuration.GetSection(InboxOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
    
    public static string GetRequiredConnectionString(
        this IConfiguration configuration,
        string name)
    {
        var value = configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required connection string: ConnectionStrings:{name}");
        }

        return value;
    }
}