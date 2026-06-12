using MediatR;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

namespace UnameIT.RevenueIntelligence.Application.Features.Dashboard.Queries;

public record GetLeadershipDashboardQuery(
    Guid TenantId,
    DateTimeOffset From,
    DateTimeOffset To
) : IRequest<LeadershipDashboardDto>;

public record LeadershipDashboardDto(
    int TotalCalls,
    int TotalOpportunities,
    decimal TotalPipelineValue,
    decimal AverageCallDurationMinutes,
    int HighRiskDeals,
    IReadOnlyList<RepPerformanceDto> TopPerformers,
    IReadOnlyList<DealAtRiskDto> DealsAtRisk,
    IReadOnlyList<CallTrendDto> CallTrend
);

public record RepPerformanceDto(
    Guid UserId,
    string Name,
    int CallCount,
    decimal AverageScore,
    int ActionItemsCompleted
);

public record DealAtRiskDto(
    Guid OpportunityId,
    string Name,
    string AccountName,
    decimal? Amount,
    int RiskScore,
    IReadOnlyList<string> RiskReasons
);

public record CallTrendDto(DateTimeOffset Date, int Count, decimal AverageScore);

public class GetLeadershipDashboardQueryHandler : IRequestHandler<GetLeadershipDashboardQuery, LeadershipDashboardDto>
{
    private readonly IUnitOfWork _uow;

    public GetLeadershipDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<LeadershipDashboardDto> Handle(GetLeadershipDashboardQuery query, CancellationToken ct)
    {
        var highRiskOpps = await _uow.Opportunities.GetHighRiskAsync(query.TenantId, 70, ct);

        var dealsAtRisk = highRiskOpps.Select(o => new DealAtRiskDto(
            o.Id, o.Name, o.Account?.Name ?? "Unknown",
            o.Amount, o.RiskScore ?? 0,
            new List<string>
            {
                o.HasNextStep ? "" : "No next step defined",
                o.HasBudgetDiscussion ? "" : "No budget discussion",
                o.IsStalled ? "Deal stalled" : ""
            }.Where(s => s.Length > 0).ToList()
        )).ToList();

        return new LeadershipDashboardDto(
            TotalCalls: 0,
            TotalOpportunities: highRiskOpps.Count,
            TotalPipelineValue: highRiskOpps.Sum(o => o.Amount ?? 0),
            AverageCallDurationMinutes: 0,
            HighRiskDeals: dealsAtRisk.Count,
            TopPerformers: new List<RepPerformanceDto>(),
            DealsAtRisk: dealsAtRisk,
            CallTrend: new List<CallTrendDto>()
        );
    }
}
