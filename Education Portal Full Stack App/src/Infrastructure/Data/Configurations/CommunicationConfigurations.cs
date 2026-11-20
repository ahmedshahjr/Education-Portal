using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolEventConfiguration : IEntityTypeConfiguration<SchoolEvent>
{
    public void Configure(EntityTypeBuilder<SchoolEvent> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(SchoolEvent.TitleMaxLength);
        builder.Property(e => e.Description).HasMaxLength(SchoolEvent.DescriptionMaxLength);
        builder.HasIndex(e => new { e.SchoolId, e.StartsAt });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.RecipientAccountId).IsRequired().HasMaxLength(Notification.RecipientMaxLength);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(SchoolEvent.TitleMaxLength);
        builder.Property(n => n.Body).HasMaxLength(SchoolEvent.DescriptionMaxLength);
        builder.HasOne(n => n.SchoolEvent).WithMany().HasForeignKey(n => n.SchoolEventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(n => new { n.SchoolId, n.RecipientAccountId, n.Created });
    }
}
