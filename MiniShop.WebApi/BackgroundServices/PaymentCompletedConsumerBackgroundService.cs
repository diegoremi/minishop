using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Events;

namespace MiniShop.WebApi.BackgroundServices;

public class PaymentCompletedConsumerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<PaymentCompletedConsumerBackgroundService> _logger;
    
    public PaymentCompletedConsumerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<PaymentCompletedConsumerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.PaymentCompletedConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        
        using var consumer  = new ConsumerBuilder<string, string>(config).Build();
        
        consumer.Subscribe(_options.PaymentCompletedTopic);
        
        _logger.LogInformation(
            "PaymentCompleted consumer started. Listening topic: {Topic}",
            _options.PaymentCompletedTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    var paymentCompletedEvent =
                        JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(
                            result.Message.Value,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                            });
                    
                    if (paymentCompletedEvent is null)
                    {
                        _logger.LogWarning("Invalid PaymentCompletedIntegrationEvent received.");
                        consumer.Commit(result);
                        continue;
                    }

                    await ProcessPaymentCompletedAsync(paymentCompletedEvent);

                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error while reading PaymentCompleted event.");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing PaymentCompleted event.");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessPaymentCompletedAsync(PaymentCompletedIntegrationEvent paymentCompletedEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        
        var order = await orderService.GetOrderByIdAsync(paymentCompletedEvent.OrderId);

        if (order is null)
        {
            _logger.LogWarning(
                "PaymentCompleted received but Order was not found. OrderId: {OrderId}",
                paymentCompletedEvent.OrderId);

            return;
        }

        if (order.Status == OrderStatus.Paid)
        {
            _logger.LogInformation(
                "PaymentCompleted received but Order is already paid. OrderId: {OrderId}",
                paymentCompletedEvent.OrderId);

            return;
        }

        if (order.Status != OrderStatus.Placed)
        {
            _logger.LogWarning(
                "PaymentCompleted received but Order is not in Placed status. OrderId: {OrderId}, Status: {Status}",
                paymentCompletedEvent.OrderId,
                order.Status);

            return;
        }
        
        await orderService.PayOrderAsync(paymentCompletedEvent.OrderId);

        _logger.LogInformation(
            "Order marked as Paid from PaymentCompleted event. OrderId: {OrderId}, Amount: {Amount}",
            paymentCompletedEvent.OrderId,
            paymentCompletedEvent.Amount);
    }
}