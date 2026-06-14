using System.ComponentModel.DataAnnotations;

namespace MiniShop.Infrastructure.Configuration;

public sealed class InboxOptions
{
    public const string SectionName = "Inbox";

    [Range(1, 24 * 30)]
    public int DuplicateMessageTtlHours { get; init; } = 24;
}