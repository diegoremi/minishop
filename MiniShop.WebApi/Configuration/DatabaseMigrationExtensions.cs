using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniShop.Infrastructure.Data;

namespace MiniShop.WebApi.Configuration;

public static class DatabaseMigrationExtensions
{
    public static async Task<WebApplication> ApplyMiniShopDatabaseMigrationsAsync(
        this WebApplication app)
    {
        if (app.Environment.IsEnvironment(EnvironmentNames.Testing))
        {
            return app;
        }

        if (app.Environment.IsProduction())
        {
            return app;
        }

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        const int maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<MiniShopDbContext>();

                var connectionString = dbContext.Database.GetConnectionString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("MiniShopDb connection string is missing.");
                }

                await EnsureDatabaseExistsAsync(connectionString, logger);

                await dbContext.Database.MigrateAsync();

                logger.LogInformation("Database migrations applied successfully.");

                return app;
            }
            catch (Exception ex)
            {
                if (attempt == maxAttempts)
                {
                    logger.LogError(
                        ex,
                        "Database migration failed after {MaxAttempts} attempts.",
                        maxAttempts);

                    throw;
                }

                logger.LogWarning(
                    ex,
                    "Database migration failed. Attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds...",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay);
            }
        }

        return app;
    }

    private static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        ILogger logger)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database name is missing from the connection string.");
        }

        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var safeDatabaseName = databaseName.Replace("]", "]]");

        var commandText = $"""
            IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL
            BEGIN
                CREATE DATABASE [{safeDatabaseName}];
            END
            """;

        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();

        logger.LogInformation("Database {DatabaseName} exists or was created.", databaseName);
    }
}