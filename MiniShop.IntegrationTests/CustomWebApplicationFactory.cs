using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        builder.ConfigureTestServices(services =>
        {
            // Remove the real SQL Server EF Core registration
            services.RemoveAll<MiniShopDbContext>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<MiniShopDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<MiniShopDbContext>>();

            // Register SQLite in-memory for integration tests
            services.AddDbContext<MiniShopDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Replace Redis with in-memory distributed cache
            services.RemoveAll<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Disable background services:
            // OutboxBackgroundService and PaymentCompletedConsumerBackgroundService
            services.RemoveAll<IHostedService>();

            // Replace Kafka publisher with fake publisher
            services.RemoveAll<IIntegrationEventPublisher>();
            services.AddSingleton<FakeIntegrationEventPublisher>();
            services.AddSingleton<IIntegrationEventPublisher>(sp =>
                sp.GetRequiredService<FakeIntegrationEventPublisher>());
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