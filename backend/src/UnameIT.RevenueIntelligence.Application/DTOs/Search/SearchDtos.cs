namespace UnameIT.RevenueIntelligence.Application.DTOs.Search;

public record SearchResultDto(
    string DocumentId,
    string Content,
    float Score,
    string SourceType,
    string? SourceId,
    IDictionary<string, string> Metadata
);

public record SemanticSearchRequest(
    string Question,
    Guid TenantId,
    Guid? TeamId = null,
    Guid? UserId = null,
    Guid? AccountId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int TopK = 10
);

public record SemanticSearchResponse(
    string Answer,
    IReadOnlyList<SearchResultDto> Sources,
    string Query
);
