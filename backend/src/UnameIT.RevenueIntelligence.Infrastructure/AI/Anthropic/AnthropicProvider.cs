using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;

namespace UnameIT.RevenueIntelligence.Infrastructure.AI.Anthropic;

public class AnthropicProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
}

public class AnthropicProvider : IAIProvider
{
    private readonly AnthropicClient _client;
    private readonly AnthropicProviderOptions _options;
    private readonly ILogger<AnthropicProvider> _logger;

    public string ProviderName => "Anthropic";
    public string DefaultModel => _options.Model;

    public AnthropicProvider(IOptions<AnthropicProviderOptions> options,
        ILogger<AnthropicProvider> logger)
    {
        _options = options.Value;
        _client = new AnthropicClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task<ConversationSummaryDto> GenerateSummaryAsync(
        string transcript, string language, string meetingContext, CancellationToken ct = default)
    {
        var prompt = $"""
            Analyze this sales meeting transcript and return a JSON summary.
            Meeting: {meetingContext}, Language: {language}

            TRANSCRIPT:
            {transcript}

            Return JSON:
            {{"executiveSummary":"...","detailedSummary":"...","meetingRecap":"...","keyTopics":"...","sentimentScore":75,"sentimentLabel":"Positive"}}
            """;

        var response = await CompleteAsync(prompt, ct);
        var result = JsonSerializer.Deserialize<JsonElement>(ExtractJson(response));

        return new ConversationSummaryDto(
            result.GetProperty("executiveSummary").GetString() ?? "",
            result.GetProperty("detailedSummary").GetString() ?? "",
            result.GetProperty("meetingRecap").GetString() ?? "",
            result.GetProperty("keyTopics").GetString() ?? "",
            result.GetProperty("sentimentScore").GetInt32(),
            result.GetProperty("sentimentLabel").GetString() ?? "Neutral");
    }

    public async Task<IReadOnlyList<ActionItemDto>> ExtractActionItemsAsync(
        string transcript, string context, CancellationToken ct = default)
    {
        var prompt = $"""
            Extract action items from this transcript as JSON array.
            Context: {context}

            TRANSCRIPT:
            {transcript}

            Return JSON array of: {{"title":"...","description":null,"assigneeName":null,"dueDate":null,"priority":2,"quote":null}}
            """;

        var response = await CompleteAsync(prompt, ct);
        return JsonSerializer.Deserialize<List<ActionItemDto>>(ExtractJson(response)) ?? [];
    }

    public async Task<DealRiskAnalysisDto> AnalyzeDealRiskAsync(
        string transcript, OpportunityContextDto opportunity, CancellationToken ct = default)
    {
        var prompt = $"""
            Analyze deal risk for opportunity '{opportunity.OpportunityName}' at account '{opportunity.AccountName}'.
            Stage: {opportunity.Stage}

            TRANSCRIPT:
            {transcript}

            Return JSON: {{"healthScore":75,"riskScore":30,"confidenceScore":80,"risks":[],"opportunities":[],"hasNextStep":true,"hasBudgetDiscussion":false,"hasDecisionMaker":true,"explanation":"..."}}
            """;

        var response = await CompleteAsync(prompt, ct);
        var j = JsonSerializer.Deserialize<JsonElement>(ExtractJson(response));

        return new DealRiskAnalysisDto(
            j.GetProperty("healthScore").GetInt32(),
            j.GetProperty("riskScore").GetInt32(),
            j.GetProperty("confidenceScore").GetInt32(),
            j.GetProperty("risks").EnumerateArray().Select(e => e.GetString()!).ToList(),
            j.GetProperty("opportunities").EnumerateArray().Select(e => e.GetString()!).ToList(),
            j.GetProperty("hasNextStep").GetBoolean(),
            j.GetProperty("hasBudgetDiscussion").GetBoolean(),
            j.GetProperty("hasDecisionMaker").GetBoolean(),
            j.GetProperty("explanation").GetString() ?? "");
    }

    public async Task<CoachingFeedbackDto> GenerateCoachingFeedbackAsync(
        string transcript, ScorecardContextDto scorecard, CancellationToken ct = default)
    {
        var prompt = $"""
            Analyze sales coaching for template '{scorecard.TemplateName}'.
            TRANSCRIPT: {transcript}
            Return JSON: {{"discoveryScore":8,"qualificationScore":7,"objectionHandlingScore":6,"closingScore":7,"communicationScore":8,"overallScore":7,"strengths":[],"improvementAreas":[],"detailedFeedback":"..."}}
            """;

        var response = await CompleteAsync(prompt, ct);
        var j = JsonSerializer.Deserialize<JsonElement>(ExtractJson(response));

        return new CoachingFeedbackDto(
            j.GetProperty("discoveryScore").GetInt32(),
            j.GetProperty("qualificationScore").GetInt32(),
            j.GetProperty("objectionHandlingScore").GetInt32(),
            j.GetProperty("closingScore").GetInt32(),
            j.GetProperty("communicationScore").GetInt32(),
            j.GetProperty("overallScore").GetInt32(),
            j.GetProperty("strengths").EnumerateArray().Select(e => e.GetString()!).ToList(),
            j.GetProperty("improvementAreas").EnumerateArray().Select(e => e.GetString()!).ToList(),
            j.GetProperty("detailedFeedback").GetString() ?? "");
    }

    public async Task<CrmUpdateSuggestionsDto> GenerateCrmUpdateAsync(
        string transcript, CrmContextDto context, CancellationToken ct = default)
    {
        var prompt = $"Generate CRM update for {context.AccountName}. TRANSCRIPT: {transcript}. Return JSON: {{\"suggestedNote\":\"...\",\"suggestedNextStep\":null,\"suggestedStageChange\":null,\"suggestedTasks\":[],\"suggestedDealUpdate\":null}}";
        var response = await CompleteAsync(prompt, ct);
        var j = JsonSerializer.Deserialize<JsonElement>(ExtractJson(response));

        return new CrmUpdateSuggestionsDto(
            j.GetProperty("suggestedNote").GetString() ?? "",
            j.TryGetProperty("suggestedNextStep", out var ns) && ns.ValueKind != JsonValueKind.Null ? ns.GetString() : null,
            j.TryGetProperty("suggestedStageChange", out var sc) && sc.ValueKind != JsonValueKind.Null ? sc.GetString() : null,
            j.GetProperty("suggestedTasks").EnumerateArray().Select(e => e.GetString()!).ToList(),
            j.TryGetProperty("suggestedDealUpdate", out var du) && du.ValueKind != JsonValueKind.Null ? du.GetString() : null);
    }

    public async Task<string> AnswerQuestionAsync(string question,
        IReadOnlyList<string> contextDocuments, CancellationToken ct = default)
    {
        var context = string.Join("\n---\n", contextDocuments.Take(8));
        var prompt = $"Answer based on these transcripts:\n{context}\n\nQuestion: {question}";
        return await CompleteAsync(prompt, ct);
    }

    public async Task<string> GenerateFollowUpEmailAsync(
        string transcript, FollowUpEmailContextDto context, CancellationToken ct = default)
    {
        var prompt = $"Write a follow-up email for {context.RecipientName} at {context.AccountName} from {context.SenderName}. TRANSCRIPT: {transcript}";
        return await CompleteAsync(prompt, ct);
    }

    public async Task<IReadOnlyList<InsightDto>> ExtractInsightsAsync(
        string transcript, CancellationToken ct = default)
    {
        var prompt = $"Extract insights as JSON array from: {transcript}. Return: [{{\"type\":\"PainPoint\",\"content\":\"...\",\"quote\":null,\"startTime\":null,\"speaker\":null,\"confidence\":0.8}}]";
        var response = await CompleteAsync(prompt, ct);
        return JsonSerializer.Deserialize<List<InsightDto>>(ExtractJson(response)) ?? [];
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        // Anthropic does not yet provide embedding APIs — return zeros placeholder
        _logger.LogWarning("Anthropic does not support embeddings; returning zero vector.");
        return Task.FromResult(new float[1536]);
    }

    private async Task<string> CompleteAsync(string prompt, CancellationToken ct)
    {
        var request = new MessageParameters
        {
            Model = _options.Model,
            MaxTokens = 4096,
            Messages = [new Message { Role = RoleType.User, Content = prompt }]
        };

        var response = await _client.Messages.GetClaudeMessageAsync(request, ct);
        return response.Content.FirstOrDefault()?.ToString() ?? string.Empty;
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        if (start == -1) start = text.IndexOf('[');
        if (start == -1) return text;
        var end = text.LastIndexOf(text[start] == '{' ? '}' : ']');
        return end == -1 ? text : text[start..(end + 1)];
    }
}
