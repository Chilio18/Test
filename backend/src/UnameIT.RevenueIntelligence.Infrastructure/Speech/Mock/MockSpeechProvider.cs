using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.Speech;

namespace UnameIT.RevenueIntelligence.Infrastructure.Speech.Mock;

public class MockSpeechProvider : ISpeechProvider
{
    public string ProviderName => "Mock";

    public Task<TranscriptionResultDto> TranscribeAsync(Stream audioStream, string language,
        TranscriptionOptionsDto options, CancellationToken ct = default)
    {
        var segments = new List<TranscriptSegmentDto>
        {
            new(0, "Sales Rep", 0m, 15m, "Goedemiddag, bedankt dat u de tijd neemt voor dit gesprek.", 0.95m),
            new(1, "Customer", 15m, 35m, "Geen probleem. Vertel me meer over jullie platform.", 0.93m),
            new(2, "Sales Rep", 35m, 80m, "Ons platform helpt salesteams om gesprekken te analyseren en deals te verbeteren.", 0.97m),
            new(3, "Customer", 80m, 120m, "Interessant. We hebben momenteel moeite met het bijhouden van klantinteracties in ons CRM.", 0.91m),
            new(4, "Sales Rep", 120m, 165m, "Dat is precies wat wij oplossen. Wat is jullie huidige CRM systeem?", 0.96m),
            new(5, "Customer", 165m, 210m, "We gebruiken een intern systeem. Budget hebben we voor Q3 beschikbaar.", 0.89m),
            new(6, "Sales Rep", 210m, 250m, "Excellent. Ik stuur u volgende week een voorstel toe.", 0.98m),
        };

        return Task.FromResult(new TranscriptionResultDto(
            language, ProviderName, 0.95m, 2, true, segments));
    }

    public Task<bool> SupportsLanguageAsync(string languageCode, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<bool> SupportsDiarizationAsync(CancellationToken ct = default)
        => Task.FromResult(false);
}
