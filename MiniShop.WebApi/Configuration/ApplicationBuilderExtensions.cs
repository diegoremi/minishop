using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace MiniShop.WebApi.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseMiniShopSwagger(
        this WebApplication app,
        IConfiguration configuration)
    {
        var swaggerEnabled =
            configuration.GetValue<bool>("Swagger:Enabled");

        if (app.Environment.IsDevelopment()
            || app.Environment.IsEnvironment("Docker")
            || app.Environment.IsEnvironment("AzureDev")
            || swaggerEnabled)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "MiniShop API v1");
            });
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