using DefaultNamespace;
using MiniShop.PaymentWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IPaymentCompletedEventPublisher, KafkaPaymentCompletedEventPublisher>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
