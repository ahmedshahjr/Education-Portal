using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class ChangeAuditEntryConfiguration : IEntityTypeConfiguration<ChangeAuditEntry>
{
    public void Configure(EntityTypeBuilder<ChangeAuditEntry> builder)
    {
        builder.ToTable("ChangeAuditEntries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.HasAlternateKey(e => e.PublicId);

        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.ActorUserId).HasMaxLength(450);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(ChangeAuditEntry.EntityTypeMaxLength);
        builder.Property(e => e.Changes).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.TraceId).HasMaxLength(64);

        // No foreign keys: an entry outlives the row it describes and never blocks a delete.
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.SchoolId);
        builder.HasIndex(e => e.OccurredAt);
    }
}
