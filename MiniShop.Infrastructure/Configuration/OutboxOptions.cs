using System.ComponentModel.DataAnnotations;

namespace MiniShop.Infrastructure.Configuration;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 300)]
    public int PollingIntervalSeconds { get; init; } = 5;

    [Range(1, 500)]
    public int BatchSize { get; init; } = 20;
}