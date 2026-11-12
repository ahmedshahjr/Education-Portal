using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class BrandingImageConfiguration : IEntityTypeConfiguration<BrandingImage>
{
    public void Configure(EntityTypeBuilder<BrandingImage> builder)
    {
        builder.Property(i => i.Kind).IsRequired(); // stored as its number (ENG-ENUM-02)
        builder.Property(i => i.ContentType).IsRequired().HasMaxLength(BrandingImage.ContentTypeMaxLength);
        builder.Property(i => i.Content).IsRequired();
        builder.Property(i => i.Version).IsRequired().HasMaxLength(BrandingImage.VersionLength);

        // One image of each kind per school; SchoolId leads the index (ENG-EF-10).
        builder.HasIndex(i => new { i.SchoolId, i.Kind }).IsUnique();

        builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(i => i.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
