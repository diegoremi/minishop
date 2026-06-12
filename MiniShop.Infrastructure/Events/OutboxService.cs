using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Data;

namespace MiniShop.Infrastructure.Events;

public class OutboxService : IOutboxService
{
    private readonly MiniShopDbContext _dbContext;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(MiniShopDbContext dbContext,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<OutboxService> logger)
    {
        _dbContext = dbContext;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }
    
    public async Task SaveAsync(IIntegrationEvent integrationEvent)
    {
        var type = integrationEvent.GetType().AssemblyQualifiedName 
                   ?? integrationEvent.GetType().FullName 
                   ?? integrationEvent.GetType().Name;
        
        var content = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType());
        
        var outboxMessage = new OutboxMessage(
            type,
            content,
            integrationEvent.OccurredOnUtc
        );


        await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();
    }

    public async Task PublishedPendingAsync()
    {
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type);
                
                if(type == null)
                    throw new InvalidOperationException($"Could not resolve type {message.Type}");
                
                var integrationEvent = JsonSerializer.Deserialize(
                    message.Content,
                    type) as IIntegrationEvent;
                
                if(integrationEvent == null)
                    throw new InvalidOperationException("Could not deserialize integration event");
                
                await _integrationEventPublisher.PublishAsync(integrationEvent);
                
                message.MarkAsProcessed();

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(
                    ex,
                    "Error publishing outbox message {OutboxMessageId}",
                    message.Id
                );

                message.MarkAsFailed(ex.Message);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}