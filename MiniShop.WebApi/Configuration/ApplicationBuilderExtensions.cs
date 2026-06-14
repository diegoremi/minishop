using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace MiniShop.WebApi.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseMiniShopSwagger(
        this WebApplication app,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Swagger:Enabled"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication MapMiniShopHealthChecks(
        this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}