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
}