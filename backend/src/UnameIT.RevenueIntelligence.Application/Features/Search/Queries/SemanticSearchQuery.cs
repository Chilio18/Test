using MediatR;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.Search;

namespace UnameIT.RevenueIntelligence.Application.Features.Search.Queries;

public record SemanticSearchQuery(
    string Question,
    Guid TenantId,
    Guid? TeamId = null,
    Guid? OwnerId = null,
    Guid? AccountId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int TopK = 10
) : IRequest<SemanticSearchResponse>;

public class SemanticSearchQueryHandler : IRequestHandler<SemanticSearchQuery, SemanticSearchResponse>
{
    private readonly IAIProvider _ai;
    private readonly IVectorSearchService _vectorSearch;

    public SemanticSearchQueryHandler(IAIProvider ai, IVectorSearchService vectorSearch)
    {
        _ai = ai;
        _vectorSearch = vectorSearch;
    }

    public async Task<SemanticSearchResponse> Handle(SemanticSearchQuery query, CancellationToken ct)
    {
        var embedding = await _ai.GenerateEmbeddingAsync(query.Question, ct);

        var filters = new Dictionary<string, string>
        {
            ["tenantId"] = query.TenantId.ToString()
        };
        if (query.OwnerId.HasValue) filters["ownerId"] = query.OwnerId.Value.ToString();
        if (query.AccountId.HasValue) filters["accountId"] = query.AccountId.Value.ToString();

        var results = await _vectorSearch.SearchAsync(embedding, query.TopK, filters, ct);

        var contextDocs = results.Select(r => r.Content).ToList();
        var answer = await _ai.AnswerQuestionAsync(query.Question, contextDocs, ct);

        return new SemanticSearchResponse(answer, results, query.Question);
    }
}
