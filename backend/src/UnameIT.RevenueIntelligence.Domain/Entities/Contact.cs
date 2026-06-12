using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Contact : TenantEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Title { get; private set; }
    public string? Department { get; private set; }
    public Guid? AccountId { get; private set; }
    public string? LinkedInUrl { get; private set; }
    public string? Notes { get; private set; }
    public string? ExternalCrmId { get; private set; }
    public string? ExternalCrmProvider { get; private set; }
    public DateTimeOffset? LastCrmSyncAt { get; private set; }
    public bool IsDecisionMaker { get; private set; }
    public bool IsChampion { get; private set; }

    public Account? Account { get; private set; }

    private Contact() : base() { }

    public static Contact Create(Guid tenantId, string firstName, string lastName,
        Guid? accountId = null, string? email = null)
    {
        return new Contact
        {
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            AccountId = accountId,
            Email = email?.ToLowerInvariant()
        };
    }

    public void Update(string firstName, string lastName, string? email, string? phone,
        string? title, string? department, bool isDecisionMaker, bool isChampion)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email?.ToLowerInvariant();
        Phone = phone;
        Title = title;
        Department = department;
        IsDecisionMaker = isDecisionMaker;
        IsChampion = isChampion;
    }

    public void SetCrmMapping(string externalId, string provider)
    {
        ExternalCrmId = externalId;
        ExternalCrmProvider = provider;
        LastCrmSyncAt = DateTimeOffset.UtcNow;
    }
}
