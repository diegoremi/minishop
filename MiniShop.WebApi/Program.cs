using MiniShop.Infrastructure.Configuration;
using MiniShop.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMiniShopConfiguration(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddMiniShopPersistence(
    builder.Configuration,
    builder.Environment);

builder.Services.AddMiniShopCaching(
    builder.Configuration,
    builder.Environment);

builder.Services.AddMiniShopApplicationServices();

builder.Services.AddMiniShopMessaging(builder.Configuration);

builder.Services.AddMiniShopBackgroundServices(
    builder.Configuration,
    builder.Environment);

builder.Services.AddMiniShopHealthChecks(
    builder.Configuration,
    builder.Environment);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiniShopSwagger(builder.Configuration);

if (!app.Environment.IsEnvironment(EnvironmentNames.Testing)
    && !app.Environment.IsEnvironment("Docker")
    && !app.Environment.IsEnvironment("AzureDev"))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapMiniShopHealthChecks();

await app.ApplyMiniShopDatabaseMigrationsAsync();

app.Run();

public partial class Program
{
}