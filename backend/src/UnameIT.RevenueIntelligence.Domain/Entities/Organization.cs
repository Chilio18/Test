using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }
    public string? Domain { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Address { get; private set; }
    public string? Country { get; private set; }
    public string? TimeZone { get; private set; }
    public string? DefaultLanguage { get; private set; }

    private readonly List<Tenant> _tenants = new();
    public IReadOnlyCollection<Tenant> Tenants => _tenants.AsReadOnly();

    private Organization() { }

    public static Organization Create(string name, string slug, string contactEmail)
    {
        var org = new Organization
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            ContactEmail = contactEmail
        };
        org.AddDomainEvent(new OrganizationCreatedEvent(org.Id, name));
        return org;
    }

    public void Update(string name, string? logoUrl, string? domain, string? timeZone)
    {
        Name = name;
        LogoUrl = logoUrl;
        Domain = domain;
        TimeZone = timeZone;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
