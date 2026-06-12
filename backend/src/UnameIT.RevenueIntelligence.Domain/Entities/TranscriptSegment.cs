using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class TranscriptSegment : TenantEntity
{
    public Guid TranscriptId { get; private set; }
    public int SequenceNumber { get; private set; }
    public string Speaker { get; private set; } = string.Empty;
    public string? SpeakerLabel { get; private set; }
    public decimal StartTime { get; private set; }
    public decimal EndTime { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public decimal? Confidence { get; private set; }
    public string? Language { get; private set; }

    public Transcript? Transcript { get; private set; }

    private TranscriptSegment() : base() { }

    public static TranscriptSegment Create(Guid tenantId, Guid transcriptId, int sequence,
        string speaker, decimal startTime, decimal endTime, string text, decimal? confidence = null)
    {
        return new TranscriptSegment
        {
            TenantId = tenantId,
            TranscriptId = transcriptId,
            SequenceNumber = sequence,
            Speaker = speaker,
            StartTime = startTime,
            EndTime = endTime,
            Text = text,
            Confidence = confidence
        };
    }

    public void SetSpeakerLabel(string label) => SpeakerLabel = label;
}
