using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Notification : TenantEntity
{
    public Guid RecipientId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Body { get; private set; }
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public User? Recipient { get; private set; }

    private Notification() : base() { }

    public static Notification Create(Guid tenantId, Guid recipientId, string type,
        string title, string? body = null, string? actionUrl = null)
    {
        return new Notification
        {
            TenantId = tenantId,
            RecipientId = recipientId,
            Type = type,
            Title = title,
            Body = body,
            ActionUrl = actionUrl
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }
}
