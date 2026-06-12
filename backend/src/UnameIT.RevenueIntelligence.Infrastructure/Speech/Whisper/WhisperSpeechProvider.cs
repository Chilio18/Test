using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.Speech;

namespace UnameIT.RevenueIntelligence.Infrastructure.Speech.Whisper;

public class WhisperOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "whisper-1";
}

public class WhisperSpeechProvider : ISpeechProvider
{
    private readonly OpenAIClient _client;
    private readonly WhisperOptions _options;
    private readonly ILogger<WhisperSpeechProvider> _logger;

    public string ProviderName => "OpenAI Whisper";

    public WhisperSpeechProvider(IOptions<WhisperOptions> options,
        ILogger<WhisperSpeechProvider> logger)
    {
        _options = options.Value;
        _client = new OpenAIClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task<TranscriptionResultDto> TranscribeAsync(Stream audioStream,
        string language, TranscriptionOptionsDto options, CancellationToken ct = default)
    {
        var audioClient = _client.GetAudioClient(_options.Model);

        var transcription = await audioClient.TranscribeAudioAsync(
            audioStream,
            "recording.mp3",
            new AudioTranscriptionOptions
            {
                Language = language,
                ResponseFormat = AudioTranscriptionFormat.Verbose,
                Temperature = 0,
                TimestampGranularities = AudioTimestampGranularities.Segment
            }, ct);

        var segments = transcription.Value.Segments
            .Select((s, i) => new TranscriptSegmentDto(
                i,
                "Speaker 1",
                (decimal)s.Start.TotalSeconds,
                (decimal)s.End.TotalSeconds,
                s.Text.Trim(),
                null))
            .ToList();

        return new TranscriptionResultDto(
            language, ProviderName, null, 1, false, segments);
    }

    public Task<bool> SupportsLanguageAsync(string languageCode, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<bool> SupportsDiarizationAsync(CancellationToken ct = default)
        => Task.FromResult(false);
}
