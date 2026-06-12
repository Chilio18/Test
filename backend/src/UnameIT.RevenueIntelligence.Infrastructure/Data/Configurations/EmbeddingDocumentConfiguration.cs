using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnameIT.RevenueIntelligence.Domain.Entities;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data.Configurations;

public class EmbeddingDocumentConfiguration : IEntityTypeConfiguration<EmbeddingDocument>
{
    public void Configure(EntityTypeBuilder<EmbeddingDocument> builder)
    {
        builder.ToTable("embedding_documents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.MetadataJson).HasColumnType("jsonb");
        // pgvector column — stored as text JSON array for now; use HasColumnType("vector(1536)") with pgvector extension
        builder.Property(e => e.EmbeddingVector).HasColumnName("embedding_vector");
        builder.HasIndex(e => new { e.TenantId, e.SourceType, e.SourceId });
    }
}
