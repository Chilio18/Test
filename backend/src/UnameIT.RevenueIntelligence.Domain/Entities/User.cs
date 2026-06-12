using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class User : TenantEntity
{
    public string ExternalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastLoginAt { get; private set; }
    public string? Title { get; private set; }
    public string? Department { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PreferencesJson { get; private set; }

    public Tenant? Tenant { get; private set; }

    private readonly List<TeamMembership> _memberships = new();
    public IReadOnlyCollection<TeamMembership> Memberships => _memberships.AsReadOnly();

    private User() : base() { }

    public static User Create(Guid tenantId, string externalId, string email,
        string firstName, string lastName, UserRole role)
    {
        var user = new User
        {
            TenantId = tenantId,
            ExternalId = externalId,
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };
        user.AddDomainEvent(new UserCreatedEvent(user.Id, tenantId, email, role));
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string? title,
        string? department, string? phoneNumber, string? avatarUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        Title = title;
        Department = department;
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
    }

    public void UpdateRole(UserRole role)
    {
        var oldRole = Role;
        Role = role;
        AddDomainEvent(new UserRoleChangedEvent(Id, TenantId, oldRole, role));
    }

    public void RecordLogin() => LastLoginAt = DateTimeOffset.UtcNow;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
