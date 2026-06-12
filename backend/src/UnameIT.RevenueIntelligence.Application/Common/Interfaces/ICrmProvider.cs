using UnameIT.RevenueIntelligence.Application.DTOs.CRM;

namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface ICrmProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<CrmAccountDto>> GetAccountsAsync(CancellationToken ct = default);
    Task<CrmAccountDto?> GetAccountAsync(string externalId, CancellationToken ct = default);

    Task<IReadOnlyList<CrmContactDto>> GetContactsAsync(CancellationToken ct = default);
    Task<CrmContactDto?> GetContactAsync(string externalId, CancellationToken ct = default);

    Task<IReadOnlyList<CrmOpportunityDto>> GetOpportunitiesAsync(CancellationToken ct = default);
    Task<CrmOpportunityDto?> GetOpportunityAsync(string externalId, CancellationToken ct = default);

    Task<string> CreateNoteAsync(CreateCrmNoteRequest request, CancellationToken ct = default);
    Task<string> CreateTaskAsync(CreateCrmTaskRequest request, CancellationToken ct = default);

    Task UpdateOpportunityAsync(string externalId, UpdateCrmOpportunityRequest request,
        CancellationToken ct = default);

    Task SyncCallInsightsAsync(SyncCallInsightsRequest request, CancellationToken ct = default);

    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}
