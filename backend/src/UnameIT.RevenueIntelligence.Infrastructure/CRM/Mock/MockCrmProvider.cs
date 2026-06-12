using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.CRM;

namespace UnameIT.RevenueIntelligence.Infrastructure.CRM.Mock;

public class MockCrmProvider : ICrmProvider
{
    public string ProviderName => "Mock CRM";

    public Task<IReadOnlyList<CrmAccountDto>> GetAccountsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CrmAccountDto>>(
        [
            new CrmAccountDto("acc-001", "TechCorp BV", "https://techcorp.nl", "Technology", "Netherlands", "+31 20 123 4567"),
            new CrmAccountDto("acc-002", "BuildIT NV", "https://buildit.be", "Construction", "Belgium", "+32 2 123 4567")
        ]);

    public Task<CrmAccountDto?> GetAccountAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmAccountDto?>(new CrmAccountDto(externalId, "TechCorp BV", null, "Technology", "Netherlands", null));

    public Task<IReadOnlyList<CrmContactDto>> GetContactsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CrmContactDto>>(
        [
            new CrmContactDto("con-001", "Jan", "de Vries", "jan@techcorp.nl", null, "CTO", "acc-001"),
            new CrmContactDto("con-002", "Maria", "Janssen", "maria@buildit.be", null, "CEO", "acc-002")
        ]);

    public Task<CrmContactDto?> GetContactAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmContactDto?>(null);

    public Task<IReadOnlyList<CrmOpportunityDto>> GetOpportunitiesAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CrmOpportunityDto>>(
        [
            new CrmOpportunityDto("opp-001", "TechCorp Platform Upgrade", "acc-001", "Proposal/Quote", 45000m, "EUR", DateTimeOffset.UtcNow.AddMonths(2), 60m),
            new CrmOpportunityDto("opp-002", "BuildIT Digital Transformation", "acc-002", "Qualification", 120000m, "EUR", DateTimeOffset.UtcNow.AddMonths(4), 30m)
        ]);

    public Task<CrmOpportunityDto?> GetOpportunityAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmOpportunityDto?>(null);

    public Task<string> CreateNoteAsync(CreateCrmNoteRequest request, CancellationToken ct = default)
        => Task.FromResult($"mock-note-{Guid.NewGuid():N}");

    public Task<string> CreateTaskAsync(CreateCrmTaskRequest request, CancellationToken ct = default)
        => Task.FromResult($"mock-task-{Guid.NewGuid():N}");

    public Task UpdateOpportunityAsync(string externalId, UpdateCrmOpportunityRequest request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SyncCallInsightsAsync(SyncCallInsightsRequest request, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> TestConnectionAsync(CancellationToken ct = default)
        => Task.FromResult(true);
}
