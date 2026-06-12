using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Tenant : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string? TimeZone { get; private set; }
    public string? DefaultLanguage { get; private set; }
    public int MaxUsers { get; private set; } = 50;
    public int MaxStorageGb { get; private set; } = 100;
    public int DataRetentionDays { get; private set; } = 365;
    public bool AiEnabled { get; private set; } = true;
    public bool CrmIntegrationEnabled { get; private set; } = true;
    public bool ConsentTrackingEnabled { get; private set; } = true;
    public string? AiSettingsJson { get; private set; }
    public string? BrandingJson { get; private set; }
    public string? FeatureFlagsJson { get; private set; }

    public Organization? Organization { get; private set; }

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<CrmConnection> _crmConnections = new();
    public IReadOnlyCollection<CrmConnection> CrmConnections => _crmConnections.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(Guid organizationId, string name, string slug)
    {
        var tenant = new Tenant
        {
            OrganizationId = organizationId,
            Name = name,
            Slug = slug.ToLowerInvariant()
        };
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, organizationId, name));
        return tenant;
    }

    public void UpdateSettings(string? timeZone, string? language, int retentionDays,
        bool aiEnabled, bool crmEnabled)
    {
        TimeZone = timeZone;
        DefaultLanguage = language;
        DataRetentionDays = retentionDays;
        AiEnabled = aiEnabled;
        CrmIntegrationEnabled = crmEnabled;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
