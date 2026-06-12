using UnameIT.RevenueIntelligence.Application.DTOs.Speech;

namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface ISpeechProvider
{
    string ProviderName { get; }

    Task<TranscriptionResultDto> TranscribeAsync(
        Stream audioStream, string language, TranscriptionOptionsDto options,
        CancellationToken ct = default);

    Task<bool> SupportsLanguageAsync(string languageCode, CancellationToken ct = default);
    Task<bool> SupportsDiarizationAsync(CancellationToken ct = default);
}
