using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SecurityAuditEventConfiguration : IEntityTypeConfiguration<SecurityAuditEvent>
{
    public void Configure(EntityTypeBuilder<SecurityAuditEvent> builder)
    {
        builder.ToTable("SecurityAuditEvents");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.OccurredAt).IsRequired();
        // Stored as their numbers (ENG-ENUM-02); the SecurityAuditEventTypes lookup table gives reports the names.
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.Outcome).IsRequired();

        // The public identity, if a record is ever exposed (ENG-ID-02).
        builder.HasAlternateKey(e => e.PublicId);

        builder.Property(e => e.ActorUserId).HasMaxLength(450);
        builder.Property(e => e.TargetType).HasMaxLength(64);
        builder.Property(e => e.TargetId).HasMaxLength(128);
        builder.Property(e => e.SourceIp).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.TraceId).HasMaxLength(64);

        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.ActorUserId);
    }
}
