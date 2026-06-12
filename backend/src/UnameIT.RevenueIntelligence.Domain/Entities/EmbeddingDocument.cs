using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class EmbeddingDocument : TenantEntity
{
    public string DocumentType { get; private set; } = string.Empty;
    public Guid? SourceId { get; private set; }
    public string? SourceType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? EmbeddingVector { get; private set; }
    public int? TokenCount { get; private set; }
    public string? EmbeddingModel { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset? EmbeddedAt { get; private set; }

    private EmbeddingDocument() : base() { }

    public static EmbeddingDocument Create(Guid tenantId, string documentType, Guid? sourceId,
        string sourceType, string content)
    {
        return new EmbeddingDocument
        {
            TenantId = tenantId,
            DocumentType = documentType,
            SourceId = sourceId,
            SourceType = sourceType,
            Content = content
        };
    }

    public void SetEmbedding(string vector, string model, int tokenCount)
    {
        EmbeddingVector = vector;
        EmbeddingModel = model;
        TokenCount = tokenCount;
        EmbeddedAt = DateTimeOffset.UtcNow;
    }
}
