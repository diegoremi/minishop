using Microsoft.EntityFrameworkCore;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Data;
using MiniShop.ApplicationCore.Services;
using MiniShop.Infrastructure.Events;
using MiniShop.WebApi.Caching;
using MiniShop.WebApi.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "MiniShop:";
});
builder.Services.AddDbContext<MiniShopDbContext>(options =>
{
    options.UseInMemoryDatabase("MiniShop");
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IIntegrationEventPublisher, KafkaIntegrationEventPublisher>();

builder.Services.AddScoped<IOutboxService, OutboxService>();

builder.Services.AddHostedService<OutboxBackgroundService>();

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
