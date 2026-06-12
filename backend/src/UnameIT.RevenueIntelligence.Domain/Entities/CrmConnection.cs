using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CrmConnection : TenantEntity
{
    public CrmProviderType ProviderType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public string? BaseUrl { get; private set; }
    public string? ClientId { get; private set; }
    public string? EncryptedClientSecret { get; private set; }
    public string? EncryptedAccessToken { get; private set; }
    public string? EncryptedRefreshToken { get; private set; }
    public DateTimeOffset? TokenExpiresAt { get; private set; }
    public string? ApiKey { get; private set; }
    public string? OrganizationId { get; private set; }
    public string? ConfigurationJson { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public string? LastSyncError { get; private set; }
    public int SyncIntervalMinutes { get; private set; } = 60;

    private readonly List<CrmFieldMapping> _fieldMappings = new();
    public IReadOnlyCollection<CrmFieldMapping> FieldMappings => _fieldMappings.AsReadOnly();

    private CrmConnection() : base() { }

    public static CrmConnection Create(Guid tenantId, CrmProviderType type, string name)
    {
        return new CrmConnection
        {
            TenantId = tenantId,
            ProviderType = type,
            Name = name
        };
    }

    public void SetOAuth2Tokens(string accessToken, string? refreshToken, DateTimeOffset? expiresAt)
    {
        EncryptedAccessToken = accessToken;
        EncryptedRefreshToken = refreshToken;
        TokenExpiresAt = expiresAt;
    }

    public void RecordSync(bool success, string? error = null)
    {
        LastSyncAt = DateTimeOffset.UtcNow;
        LastSyncError = success ? null : error;
    }

    public bool NeedsTokenRefresh => TokenExpiresAt.HasValue &&
        TokenExpiresAt.Value < DateTimeOffset.UtcNow.AddMinutes(5);
}
