using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Account : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public string? Industry { get; private set; }
    public string? Size { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public string? Description { get; private set; }
    public string? ExternalCrmId { get; private set; }
    public string? ExternalCrmProvider { get; private set; }
    public DateTimeOffset? LastCrmSyncAt { get; private set; }
    public decimal? AnnualRevenue { get; private set; }
    public int? EmployeeCount { get; private set; }
    public Guid? OwnerId { get; private set; }

    public User? Owner { get; private set; }

    private readonly List<Contact> _contacts = new();
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    private readonly List<Opportunity> _opportunities = new();
    public IReadOnlyCollection<Opportunity> Opportunities => _opportunities.AsReadOnly();

    private readonly List<Call> _calls = new();
    public IReadOnlyCollection<Call> Calls => _calls.AsReadOnly();

    private Account() : base() { }

    public static Account Create(Guid tenantId, string name, Guid? ownerId = null)
    {
        return new Account
        {
            TenantId = tenantId,
            Name = name,
            OwnerId = ownerId
        };
    }

    public void Update(string name, string? website, string? industry, string? size,
        string? country, string? city, string? description)
    {
        Name = name;
        Website = website;
        Industry = industry;
        Size = size;
        Country = country;
        City = city;
        Description = description;
    }

    public void SetCrmMapping(string externalId, string provider)
    {
        ExternalCrmId = externalId;
        ExternalCrmProvider = provider;
        LastCrmSyncAt = DateTimeOffset.UtcNow;
    }
}
