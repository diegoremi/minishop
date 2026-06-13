using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniShop.ApplicationCore.Entities;
using MiniShop.ApplicationCore.IntegrationEvents;
using MiniShop.ApplicationCore.Interfaces;
using MiniShop.Infrastructure.Data;

namespace MiniShop.Infrastructure.Events;

public sealed class InboxService : IInboxService
{
    private readonly MiniShopDbContext _dbContext;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public InboxService(MiniShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> TryRegisterAsync(
        IIntegrationEvent integrationEvent, 
        string consumer,
        CancellationToken cancellationToken = default)
    {
        var existingMessage = await _dbContext.InboxMessages
            .FirstOrDefaultAsync(
                x => x.EventId == integrationEvent.EventId &&
                     x.Consumer == consumer,
                cancellationToken);

        if (existingMessage is not null)
            return existingMessage.ProcessedOnUtc is null;

        var content = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            JsonOptions
        );

        var inboxMessage = new InboxMessage(
            eventId: integrationEvent.EventId,
            type: integrationEvent.GetType().AssemblyQualifiedName!,
            content: content,
            consumer: consumer
        );
        
        _dbContext.InboxMessages.Add(inboxMessage);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task MarkAsProcessedAsync(
        Guid eventId, 
        string consumer, 
        CancellationToken cancellationToken = default)
    {
        var inboxMessage = await _dbContext.InboxMessages
            .FirstOrDefaultAsync(
                x => x.EventId == eventId &&
                     x.Consumer == consumer,
                cancellationToken);

        if (inboxMessage is null)
            return;
        
        inboxMessage.MarkAsProcessed();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        Guid eventId, 
        string consumer, 
        string error, 
        CancellationToken cancellationToken = default)
    {
        var inboxMessage = await _dbContext.InboxMessages
            .FirstOrDefaultAsync(
                x => x.EventId == eventId &&
                     x.Consumer == consumer,
                cancellationToken);

        if (inboxMessage is null)
        {
            return;
        }

        inboxMessage.MarkAsFailed(error);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}