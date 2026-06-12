using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnameIT.RevenueIntelligence.Domain.Entities;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data.Configurations;

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("opportunities");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).HasMaxLength(500).IsRequired();
        builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.DealScoreExplanationJson).HasColumnType("jsonb");
        builder.HasIndex(o => new { o.TenantId, o.Stage });
        builder.HasIndex(o => new { o.TenantId, o.OwnerId });
        builder.HasIndex(o => o.ExternalCrmId);

        builder.HasOne(o => o.Account)
            .WithMany(a => a.Opportunities)
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
