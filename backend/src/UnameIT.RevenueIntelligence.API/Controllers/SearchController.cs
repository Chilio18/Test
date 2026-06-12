using Microsoft.AspNetCore.Mvc;
using UnameIT.RevenueIntelligence.Application.DTOs.Search;
using UnameIT.RevenueIntelligence.Application.Features.Search.Queries;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Search")]
public class SearchController : BaseController
{
    /// <summary>Ask a natural language question across all call transcripts (RAG)</summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(SemanticSearchResponse), 200)]
    public async Task<IActionResult> Ask(
        [FromBody] AskRequest request, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SemanticSearchQuery(
            request.Question, TenantId,
            request.TeamId, request.OwnerId, request.AccountId,
            request.From, request.To, request.TopK), ct);
        return Ok(result);
    }
}

public record AskRequest(
    string Question,
    Guid? TeamId = null,
    Guid? OwnerId = null,
    Guid? AccountId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int TopK = 10
);
