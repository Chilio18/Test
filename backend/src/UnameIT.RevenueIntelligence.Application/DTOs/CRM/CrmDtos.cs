namespace UnameIT.RevenueIntelligence.Application.DTOs.CRM;

public record CrmAccountDto(
    string ExternalId,
    string Name,
    string? Website,
    string? Industry,
    string? Country,
    string? Phone
);

public record CrmContactDto(
    string ExternalId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Title,
    string? AccountId
);

public record CrmOpportunityDto(
    string ExternalId,
    string Name,
    string? AccountId,
    string? Stage,
    decimal? Amount,
    string? Currency,
    DateTimeOffset? CloseDate,
    decimal? Probability
);

public record CreateCrmNoteRequest(
    string? AccountId,
    string? ContactId,
    string? OpportunityId,
    string Title,
    string Body,
    DateTimeOffset NoteDate
);

public record CreateCrmTaskRequest(
    string? AccountId,
    string? ContactId,
    string? OpportunityId,
    string? AssigneeId,
    string Subject,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority = "Normal"
);

public record UpdateCrmOpportunityRequest(
    string? Stage,
    decimal? Amount,
    DateTimeOffset? CloseDate,
    decimal? Probability,
    string? Description,
    string? NextStep
);

public record SyncCallInsightsRequest(
    string? AccountId,
    string? OpportunityId,
    string CallTitle,
    DateTimeOffset CallDate,
    string Summary,
    IReadOnlyList<string> ActionItems,
    int? DealHealthScore,
    int? RiskScore
);
