namespace UnameIT.RevenueIntelligence.Application.DTOs.Speech;

public record TranscriptionResultDto(
    string Language,
    string Provider,
    decimal? Confidence,
    int SpeakerCount,
    bool IsDiarized,
    IReadOnlyList<TranscriptSegmentDto> Segments
);

public record TranscriptSegmentDto(
    int Sequence,
    string Speaker,
    decimal StartTime,
    decimal EndTime,
    string Text,
    decimal? Confidence
);

public record TranscriptionOptionsDto(
    bool EnableDiarization = true,
    int? ExpectedSpeakers = null,
    string? Language = null,
    bool EnablePunctuation = true,
    bool EnableWordTimestamps = false
);
