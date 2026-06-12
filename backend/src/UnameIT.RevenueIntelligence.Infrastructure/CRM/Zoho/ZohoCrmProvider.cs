using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.CRM;

namespace UnameIT.RevenueIntelligence.Infrastructure.CRM.Zoho;

public class ZohoOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.zohoapis.eu/crm/v5";
    public string OAuthUrl { get; set; } = "https://accounts.zoho.eu/oauth/v2/token";
}

public class ZohoCrmProvider : ICrmProvider
{
    private readonly HttpClient _http;
    private readonly ZohoOptions _options;
    private readonly ILogger<ZohoCrmProvider> _logger;
    private string? _accessToken;
    private DateTimeOffset _tokenExpiry;

    public string ProviderName => "Zoho CRM";

    public ZohoCrmProvider(IHttpClientFactory factory, IOptions<ZohoOptions> options,
        ILogger<ZohoCrmProvider> logger)
    {
        _http = factory.CreateClient("Zoho");
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CrmAccountDto>> GetAccountsAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await _http.GetAsync($"{_options.BaseUrl}/Accounts?fields=id,Account_Name,Website,Industry,Billing_Country,Phone", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);

        return doc.GetProperty("data").EnumerateArray()
            .Select(a => new CrmAccountDto(
                a.GetProperty("id").GetString() ?? "",
                a.GetProperty("Account_Name").GetString() ?? "",
                a.TryGetProperty("Website", out var w) ? w.GetString() : null,
                a.TryGetProperty("Industry", out var i) ? i.GetString() : null,
                a.TryGetProperty("Billing_Country", out var c) ? c.GetString() : null,
                a.TryGetProperty("Phone", out var p) ? p.GetString() : null))
            .ToList();
    }

    public async Task<CrmAccountDto?> GetAccountAsync(string externalId, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await _http.GetAsync($"{_options.BaseUrl}/Accounts/{externalId}", ct);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(ct);
        var a = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("data")[0];
        return new CrmAccountDto(
            a.GetProperty("id").GetString() ?? "",
            a.GetProperty("Account_Name").GetString() ?? "",
            a.TryGetProperty("Website", out var w) ? w.GetString() : null,
            a.TryGetProperty("Industry", out var i) ? i.GetString() : null,
            null, null);
    }

    public async Task<IReadOnlyList<CrmContactDto>> GetContactsAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await _http.GetAsync($"{_options.BaseUrl}/Contacts?fields=id,First_Name,Last_Name,Email,Phone,Title,Account_Name", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        return doc.GetProperty("data").EnumerateArray()
            .Select(c => new CrmContactDto(
                c.GetProperty("id").GetString() ?? "",
                c.GetProperty("First_Name").GetString() ?? "",
                c.GetProperty("Last_Name").GetString() ?? "",
                c.TryGetProperty("Email", out var e) ? e.GetString() : null,
                c.TryGetProperty("Phone", out var p) ? p.GetString() : null,
                c.TryGetProperty("Title", out var t) ? t.GetString() : null,
                null))
            .ToList();
    }

    public Task<CrmContactDto?> GetContactAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmContactDto?>(null);

    public async Task<IReadOnlyList<CrmOpportunityDto>> GetOpportunitiesAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await _http.GetAsync($"{_options.BaseUrl}/Deals?fields=id,Deal_Name,Account_Name,Stage,Amount,Currency,Closing_Date,Probability", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        return doc.GetProperty("data").EnumerateArray()
            .Select(d => new CrmOpportunityDto(
                d.GetProperty("id").GetString() ?? "",
                d.GetProperty("Deal_Name").GetString() ?? "",
                null,
                d.TryGetProperty("Stage", out var s) ? s.GetString() : null,
                d.TryGetProperty("Amount", out var a) && a.ValueKind == JsonValueKind.Number ? a.GetDecimal() : null,
                d.TryGetProperty("Currency", out var cur) ? cur.GetString() : "EUR",
                null,
                d.TryGetProperty("Probability", out var prob) && prob.ValueKind == JsonValueKind.Number ? prob.GetDecimal() : null))
            .ToList();
    }

    public Task<CrmOpportunityDto?> GetOpportunityAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmOpportunityDto?>(null);

    public async Task<string> CreateNoteAsync(CreateCrmNoteRequest request, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var payload = JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new
                {
                    Note_Title = request.Title,
                    Note_Content = request.Body,
                    Parent_Id = request.OpportunityId ?? request.AccountId,
                    se_module = request.OpportunityId != null ? "Deals" : "Accounts"
                }
            }
        });

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{_options.BaseUrl}/Notes", content, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        return doc.GetProperty("data")[0].GetProperty("details").GetProperty("id").GetString() ?? "";
    }

    public async Task<string> CreateTaskAsync(CreateCrmTaskRequest request, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var payload = JsonSerializer.Serialize(new
        {
            data = new[] { new { Subject = request.Subject, Description = request.Description, Due_Date = request.DueDate?.ToString("yyyy-MM-dd"), Priority = request.Priority } }
        });

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{_options.BaseUrl}/Tasks", content, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        return doc.GetProperty("data")[0].GetProperty("details").GetProperty("id").GetString() ?? "";
    }

    public async Task UpdateOpportunityAsync(string externalId, UpdateCrmOpportunityRequest request,
        CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var fields = new Dictionary<string, object?>();
        if (request.Stage != null) fields["Stage"] = request.Stage;
        if (request.Amount.HasValue) fields["Amount"] = request.Amount;
        if (request.CloseDate.HasValue) fields["Closing_Date"] = request.CloseDate.Value.ToString("yyyy-MM-dd");
        if (request.NextStep != null) fields["Next_Step"] = request.NextStep;

        var payload = JsonSerializer.Serialize(new { data = new[] { fields } });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await _http.PutAsync($"{_options.BaseUrl}/Deals/{externalId}", content, ct);
    }

    public async Task SyncCallInsightsAsync(SyncCallInsightsRequest request, CancellationToken ct = default)
    {
        await CreateNoteAsync(new CreateCrmNoteRequest(
            request.AccountId, null, request.OpportunityId,
            $"Call: {request.CallTitle}",
            request.Summary,
            request.CallDate), ct);
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            await EnsureTokenAsync(ct);
            var response = await _http.GetAsync($"{_options.BaseUrl}/org", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (_accessToken != null && _tokenExpiry > DateTimeOffset.UtcNow.AddMinutes(5)) return;

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = _options.RefreshToken
        });

        var response = await _http.PostAsync(_options.OAuthUrl, tokenRequest, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);

        _accessToken = doc.GetProperty("access_token").GetString();
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(doc.GetProperty("expires_in").GetInt32());
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", _accessToken);
    }
}
