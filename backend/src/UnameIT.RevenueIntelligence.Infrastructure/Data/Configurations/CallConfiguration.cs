using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnameIT.RevenueIntelligence.Domain.Entities;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data.Configurations;

public class CallConfiguration : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.ToTable("calls");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).HasMaxLength(500).IsRequired();
        builder.Property(c => c.MetadataJson).HasColumnType("jsonb");
        builder.HasIndex(c => new { c.TenantId, c.MeetingDate });
        builder.HasIndex(c => new { c.TenantId, c.OwnerId });
        builder.HasIndex(c => new { c.TenantId, c.Status });

        builder.HasOne(c => c.Account)
            .WithMany(a => a.Calls)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Opportunity)
            .WithMany(o => o.Calls)
            .HasForeignKey(c => c.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Recordings)
            .WithOne(r => r.Call)
            .HasForeignKey(r => r.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Transcripts)
            .WithOne(t => t.Call)
            .HasForeignKey(t => t.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Insights)
            .WithOne(i => i.Call)
            .HasForeignKey(i => i.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ActionItems)
            .WithOne(a => a.Call)
            .HasForeignKey(a => a.CallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
