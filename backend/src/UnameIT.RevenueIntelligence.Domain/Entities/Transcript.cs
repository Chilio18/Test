using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Transcript : TenantEntity
{
    public Guid CallId { get; private set; }
    public string Language { get; private set; } = "nl";
    public string? Provider { get; private set; }
    public string? ModelVersion { get; private set; }
    public decimal? ConfidenceScore { get; private set; }
    public int SpeakerCount { get; private set; }
    public bool IsDiarized { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public Call? Call { get; private set; }

    private readonly List<TranscriptSegment> _segments = new();
    public IReadOnlyCollection<TranscriptSegment> Segments => _segments.AsReadOnly();

    private Transcript() : base() { }

    public static Transcript Create(Guid tenantId, Guid callId, string language, string provider)
    {
        return new Transcript
        {
            TenantId = tenantId,
            CallId = callId,
            Language = language,
            Provider = provider,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetDiarization(int speakerCount)
    {
        IsDiarized = true;
        SpeakerCount = speakerCount;
    }
}
