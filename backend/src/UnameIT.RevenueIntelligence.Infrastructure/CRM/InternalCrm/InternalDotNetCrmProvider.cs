using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.CRM;

namespace UnameIT.RevenueIntelligence.Infrastructure.CRM.InternalCrm;

public class InternalCrmOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string AuthType { get; set; } = "ApiKey";
    public FieldMappings FieldMappings { get; set; } = new();
}

public class FieldMappings
{
    public string AccountsEndpoint { get; set; } = "/api/accounts";
    public string ContactsEndpoint { get; set; } = "/api/contacts";
    public string OpportunitiesEndpoint { get; set; } = "/api/opportunities";
    public string NotesEndpoint { get; set; } = "/api/notes";
    public string TasksEndpoint { get; set; } = "/api/tasks";
}

public class InternalDotNetCrmProvider : ICrmProvider
{
    private readonly HttpClient _http;
    private readonly InternalCrmOptions _options;
    private readonly ILogger<InternalDotNetCrmProvider> _logger;

    public string ProviderName => "Internal .NET CRM";

    public InternalDotNetCrmProvider(IHttpClientFactory factory,
        IOptions<InternalCrmOptions> options, ILogger<InternalDotNetCrmProvider> logger)
    {
        _http = factory.CreateClient("InternalCrm");
        _options = options.Value;
        _logger = logger;
        ConfigureAuth();
    }

    private void ConfigureAuth()
    {
        if (_options.AuthType == "ApiKey" && !string.IsNullOrEmpty(_options.ApiKey))
            _http.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
    }

    public async Task<IReadOnlyList<CrmAccountDto>> GetAccountsAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"{_options.BaseUrl}{_options.FieldMappings.AccountsEndpoint}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<JsonElement>(json);

        return items.EnumerateArray()
            .Select(a => new CrmAccountDto(
                GetString(a, "id", "Id", "accountId"),
                GetString(a, "name", "Name", "accountName") ?? "",
                GetString(a, "website", "Website"),
                GetString(a, "industry", "Industry"),
                GetString(a, "country", "Country"),
                GetString(a, "phone", "Phone")))
            .ToList();
    }

    public Task<CrmAccountDto?> GetAccountAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmAccountDto?>(null);

    public async Task<IReadOnlyList<CrmContactDto>> GetContactsAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"{_options.BaseUrl}{_options.FieldMappings.ContactsEndpoint}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<JsonElement>(json);

        return items.EnumerateArray()
            .Select(c => new CrmContactDto(
                GetString(c, "id", "Id") ?? "",
                GetString(c, "firstName", "FirstName", "first_name") ?? "",
                GetString(c, "lastName", "LastName", "last_name") ?? "",
                GetString(c, "email", "Email"),
                GetString(c, "phone", "Phone"),
                GetString(c, "title", "Title", "jobTitle"),
                GetString(c, "accountId", "AccountId")))
            .ToList();
    }

    public Task<CrmContactDto?> GetContactAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmContactDto?>(null);

    public async Task<IReadOnlyList<CrmOpportunityDto>> GetOpportunitiesAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"{_options.BaseUrl}{_options.FieldMappings.OpportunitiesEndpoint}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<JsonElement>(json);

        return items.EnumerateArray()
            .Select(o => new CrmOpportunityDto(
                GetString(o, "id", "Id") ?? "",
                GetString(o, "name", "Name", "title") ?? "",
                GetString(o, "accountId", "AccountId"),
                GetString(o, "stage", "Stage", "status"),
                null, "EUR", null, null))
            .ToList();
    }

    public Task<CrmOpportunityDto?> GetOpportunityAsync(string externalId, CancellationToken ct = default)
        => Task.FromResult<CrmOpportunityDto?>(null);

    public async Task<string> CreateNoteAsync(CreateCrmNoteRequest request, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            title = request.Title,
            body = request.Body,
            accountId = request.AccountId,
            opportunityId = request.OpportunityId,
            date = request.NoteDate
        });
        var response = await _http.PostAsync(
            $"{_options.BaseUrl}{_options.FieldMappings.NotesEndpoint}",
            new StringContent(payload, Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<JsonElement>(json)
            .TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
    }

    public async Task<string> CreateTaskAsync(CreateCrmTaskRequest request, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            subject = request.Subject,
            description = request.Description,
            dueDate = request.DueDate,
            accountId = request.AccountId,
            opportunityId = request.OpportunityId
        });
        var response = await _http.PostAsync(
            $"{_options.BaseUrl}{_options.FieldMappings.TasksEndpoint}",
            new StringContent(payload, Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<JsonElement>(json)
            .TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
    }

    public Task UpdateOpportunityAsync(string externalId, UpdateCrmOpportunityRequest request,
        CancellationToken ct = default) => Task.CompletedTask;

    public Task SyncCallInsightsAsync(SyncCallInsightsRequest request, CancellationToken ct = default)
        => CreateNoteAsync(new CreateCrmNoteRequest(
            request.AccountId, null, request.OpportunityId,
            $"Call: {request.CallTitle}", request.Summary, request.CallDate), ct)
            .ContinueWith(_ => { }, ct);

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{_options.BaseUrl}/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    private static string? GetString(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
            if (el.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String)
                return val.GetString();
        return null;
    }
}
