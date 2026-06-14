using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Data;
using MiniShop.IntegrationTests.Fakes;

namespace MiniShop.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MiniShopDb"] = "Data Source=:memory:",
                ["ConnectionStrings:Redis"] = "fake-redis",
                ["Kafka:BootstrapServers"] = "fake-kafka",
                ["Kafka:OrderPlacedTopic"] = "minishop.order-placed.v1",
                ["Kafka:PaymentCompletedTopic"] = "minishop.payment-completed.v1",
                ["Kafka:OrderPaidTopic"] = "minishop.order-paid.v1",
                ["Kafka:PaymentWorkerConsumerGroup"] = "minishop-payment-worker-tests",
                ["Kafka:WebApiConsumerGroup"] = "minishop-webapi-tests",
                ["Swagger:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<MiniShopDbContext>>();

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddSingleton(connection);

            services.AddDbContext<MiniShopDbContext>(options =>
            {
                options.UseSqlite(connection);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MiniShopDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MiniShopDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}