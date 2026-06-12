using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;

namespace UnameIT.RevenueIntelligence.Infrastructure.AI.OpenAI;

public class OpenAIProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string EmbeddingModel { get; set; } = "text-embedding-3-large";
}

public class OpenAIProvider : IAIProvider
{
    private readonly OpenAIClient _client;
    private readonly OpenAIProviderOptions _options;
    private readonly ILogger<OpenAIProvider> _logger;

    public string ProviderName => "OpenAI";
    public string DefaultModel => _options.Model;

    public OpenAIProvider(IOptions<OpenAIProviderOptions> options, ILogger<OpenAIProvider> logger)
    {
        _options = options.Value;
        _client = new OpenAIClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task<ConversationSummaryDto> GenerateSummaryAsync(
        string transcript, string language, string meetingContext, CancellationToken ct = default)
    {
        var prompt = $"""
            You are an expert sales conversation analyst. Analyze this sales meeting transcript and provide a structured summary.
            Meeting context: {meetingContext}
            Language of meeting: {language}

            TRANSCRIPT:
            {transcript}

            Respond with valid JSON matching this exact structure:
            {{
              "executiveSummary": "2-3 sentence high-level summary",
              "detailedSummary": "detailed multi-paragraph summary",
              "meetingRecap": "brief bullet-point recap suitable for CRM note",
              "keyTopics": "comma-separated list of key topics",
              "sentimentScore": 75,
              "sentimentLabel": "Positive|Neutral|Negative"
            }}
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

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
            Extract all action items and next steps from this sales meeting transcript.
            Context: {context}

            TRANSCRIPT:
            {transcript}

            Respond with valid JSON array:
            [
              {{
                "title": "Clear action item title",
                "description": "Optional details",
                "assigneeName": "Person responsible or null",
                "dueDate": "ISO date string or null",
                "priority": 1-3,
                "quote": "Direct quote from transcript that implies this action or null"
              }}
            ]
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        return JsonSerializer.Deserialize<List<ActionItemDto>>(response) ?? new List<ActionItemDto>();
    }

    public async Task<DealRiskAnalysisDto> AnalyzeDealRiskAsync(
        string transcript, OpportunityContextDto opportunity, CancellationToken ct = default)
    {
        var prompt = $"""
            Analyze the deal health and risk for this sales opportunity based on the meeting transcript.

            Opportunity: {opportunity.OpportunityName}
            Account: {opportunity.AccountName}
            Stage: {opportunity.Stage}
            Amount: {opportunity.Amount}

            TRANSCRIPT:
            {transcript}

            Respond with valid JSON:
            {{
              "healthScore": 0-100,
              "riskScore": 0-100,
              "confidenceScore": 0-100,
              "risks": ["risk1", "risk2"],
              "opportunities": ["opportunity1"],
              "hasNextStep": true/false,
              "hasBudgetDiscussion": true/false,
              "hasDecisionMaker": true/false,
              "explanation": "Detailed explanation"
            }}
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        return new DealRiskAnalysisDto(
            result.GetProperty("healthScore").GetInt32(),
            result.GetProperty("riskScore").GetInt32(),
            result.GetProperty("confidenceScore").GetInt32(),
            result.GetProperty("risks").EnumerateArray().Select(e => e.GetString()!).ToList(),
            result.GetProperty("opportunities").EnumerateArray().Select(e => e.GetString()!).ToList(),
            result.GetProperty("hasNextStep").GetBoolean(),
            result.GetProperty("hasBudgetDiscussion").GetBoolean(),
            result.GetProperty("hasDecisionMaker").GetBoolean(),
            result.GetProperty("explanation").GetString() ?? "");
    }

    public async Task<CoachingFeedbackDto> GenerateCoachingFeedbackAsync(
        string transcript, ScorecardContextDto scorecard, CancellationToken ct = default)
    {
        var criteriaList = string.Join("\n", scorecard.Criteria.Select((c, i) => $"{i + 1}. {c}"));
        var prompt = $"""
            You are a sales coaching expert. Analyze this sales call and provide coaching feedback.
            Scorecard template: {scorecard.TemplateName}
            Criteria:
            {criteriaList}

            TRANSCRIPT:
            {transcript}

            Respond with valid JSON:
            {{
              "discoveryScore": 0-10,
              "qualificationScore": 0-10,
              "objectionHandlingScore": 0-10,
              "closingScore": 0-10,
              "communicationScore": 0-10,
              "overallScore": 0-10,
              "strengths": ["strength1", "strength2"],
              "improvementAreas": ["area1", "area2"],
              "detailedFeedback": "Multi-paragraph coaching feedback"
            }}
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        return new CoachingFeedbackDto(
            result.GetProperty("discoveryScore").GetInt32(),
            result.GetProperty("qualificationScore").GetInt32(),
            result.GetProperty("objectionHandlingScore").GetInt32(),
            result.GetProperty("closingScore").GetInt32(),
            result.GetProperty("communicationScore").GetInt32(),
            result.GetProperty("overallScore").GetInt32(),
            result.GetProperty("strengths").EnumerateArray().Select(e => e.GetString()!).ToList(),
            result.GetProperty("improvementAreas").EnumerateArray().Select(e => e.GetString()!).ToList(),
            result.GetProperty("detailedFeedback").GetString() ?? "");
    }

    public async Task<CrmUpdateSuggestionsDto> GenerateCrmUpdateAsync(
        string transcript, CrmContextDto context, CancellationToken ct = default)
    {
        var prompt = $"""
            Based on this sales call transcript, generate CRM update suggestions.
            Account: {context.AccountName}
            Contact: {context.ContactName}
            Opportunity: {context.OpportunityName}
            Current Stage: {context.CurrentStage}

            TRANSCRIPT:
            {transcript}

            Respond with valid JSON:
            {{
              "suggestedNote": "CRM note text",
              "suggestedNextStep": "Suggested next step or null",
              "suggestedStageChange": "New stage or null",
              "suggestedTasks": ["task1", "task2"],
              "suggestedDealUpdate": "Deal update notes or null"
            }}
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        var result = JsonSerializer.Deserialize<JsonElement>(response);

        return new CrmUpdateSuggestionsDto(
            result.GetProperty("suggestedNote").GetString() ?? "",
            result.TryGetProperty("suggestedNextStep", out var ns) && ns.ValueKind != JsonValueKind.Null ? ns.GetString() : null,
            result.TryGetProperty("suggestedStageChange", out var sc) && sc.ValueKind != JsonValueKind.Null ? sc.GetString() : null,
            result.GetProperty("suggestedTasks").EnumerateArray().Select(e => e.GetString()!).ToList(),
            result.TryGetProperty("suggestedDealUpdate", out var du) && du.ValueKind != JsonValueKind.Null ? du.GetString() : null);
    }

    public async Task<string> AnswerQuestionAsync(string question,
        IReadOnlyList<string> contextDocuments, CancellationToken ct = default)
    {
        var context = string.Join("\n\n---\n\n", contextDocuments.Take(10));
        var prompt = $"""
            You are a sales intelligence assistant. Answer the question based solely on the provided conversation transcripts.
            If the answer is not in the transcripts, say so clearly.

            QUESTION: {question}

            RELEVANT TRANSCRIPTS:
            {context}

            Provide a clear, concise answer with specific references to the transcripts.
            """;

        return await GetChatCompletionAsync(prompt, ct);
    }

    public async Task<string> GenerateFollowUpEmailAsync(string transcript,
        FollowUpEmailContextDto context, CancellationToken ct = default)
    {
        var prompt = $"""
            Write a professional follow-up email based on this sales call.
            Recipient: {context.RecipientName} <{context.RecipientEmail}>
            Sender: {context.SenderName}
            Account: {context.AccountName}
            Opportunity: {context.OpportunityName}

            CALL TRANSCRIPT:
            {transcript}

            Write a concise, professional follow-up email that:
            1. Thanks them for their time
            2. Summarizes key discussion points
            3. Lists agreed next steps
            4. Has a clear call to action
            Return only the email body (no subject line).
            """;

        return await GetChatCompletionAsync(prompt, ct);
    }

    public async Task<IReadOnlyList<InsightDto>> ExtractInsightsAsync(
        string transcript, CancellationToken ct = default)
    {
        var prompt = $"""
            Extract key insights from this sales transcript. For each insight provide:
            - Type: PainPoint, Objection, BuyingSignal, CompetitorMention, BudgetDiscussion, TechnicalRequirement, Risk, Opportunity

            TRANSCRIPT:
            {transcript}

            Respond with JSON array:
            [
              {{
                "type": "PainPoint",
                "content": "Description of insight",
                "quote": "Direct quote or null",
                "startTime": null,
                "speaker": "Speaker name or null",
                "confidence": 0.85
              }}
            ]
            """;

        var response = await GetChatCompletionAsync(prompt, ct);
        return JsonSerializer.Deserialize<List<InsightDto>>(response) ?? new List<InsightDto>();
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var client = _client.GetEmbeddingClient(_options.EmbeddingModel);
        var result = await client.GenerateEmbeddingAsync(text, null, ct);
        return result.Value.ToFloats().ToArray();
    }

    private async Task<string> GetChatCompletionAsync(string prompt, CancellationToken ct)
    {
        var chatClient = _client.GetChatClient(_options.Model);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a sales intelligence AI assistant. Always respond with valid JSON when asked for structured data."),
            new UserChatMessage(prompt)
        };

        var completion = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            Temperature = 0.2f,
            MaxOutputTokenCount = 4096
        }, ct);

        return completion.Value.Content[0].Text;
    }
}
