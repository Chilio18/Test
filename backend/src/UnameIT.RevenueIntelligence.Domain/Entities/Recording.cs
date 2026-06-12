using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Recording : TenantEntity
{
    public Guid CallId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string? StorageBucket { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public bool IsVideo { get; private set; }
    public string? ThumbnailKey { get; private set; }
    public int? DurationSeconds { get; private set; }
    public bool IsEncrypted { get; private set; } = true;
    public string? ChecksumMd5 { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public Call? Call { get; private set; }

    private Recording() : base() { }

    public static Recording Create(Guid tenantId, Guid callId, string fileName,
        string storageKey, string contentType, long fileSizeBytes, bool isVideo)
    {
        return new Recording
        {
            TenantId = tenantId,
            CallId = callId,
            FileName = fileName,
            StorageKey = storageKey,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            IsVideo = isVideo
        };
    }

    public void SetDuration(int seconds) => DurationSeconds = seconds;
    public void SetExpiry(DateTimeOffset expiresAt) => ExpiresAt = expiresAt;
    public void SetThumbnail(string thumbnailKey) => ThumbnailKey = thumbnailKey;
}
