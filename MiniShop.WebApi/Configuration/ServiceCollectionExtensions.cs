using Microsoft.EntityFrameworkCore;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.ApplicationCore.Services;
using MiniShop.Infrastructure.Configuration;
using MiniShop.Infrastructure.Data;
using MiniShop.Infrastructure.Events;
using MiniShop.WebApi.BackgroundServices;
using MiniShop.WebApi.Caching;

namespace MiniShop.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniShopPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return services;
        }

        var sqlConnectionString =
            configuration.GetRequiredConnectionString("MiniShopDb");

        services.AddDbContext<MiniShopDbContext>(options =>
        {
            options.UseSqlServer(sqlConnectionString);
        });
        
        return services;
    }
    
    public static IServiceCollection AddMiniShopCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            var redisConnectionString =
                configuration.GetRequiredConnectionString("Redis");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "MiniShop:";
            });
        }

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddMiniShopApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();

        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IInboxService, InboxService>();
        
        return services;
    }

    public static IServiceCollection AddMiniShopMessaging(
        this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventPublisher, KafkaIntegrationEventPublisher>();
        services.AddSingleton<IDeadLetterPublisher, KafkaDeadLetterPublisher>();

        return services;
    }

    public static IServiceCollection AddMiniShopBackgroundServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return services;
        }

        services.AddHostedService<OutboxBackgroundService>();
        services.AddHostedService<PaymentCompletedConsumerBackgroundService>();

        return services;
    }
}