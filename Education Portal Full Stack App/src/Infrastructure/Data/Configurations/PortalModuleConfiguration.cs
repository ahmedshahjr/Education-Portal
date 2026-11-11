using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class PortalModuleConfiguration : IEntityTypeConfiguration<PortalModule>
{
    public void Configure(EntityTypeBuilder<PortalModule> builder)
    {
        builder.ToTable("Modules");

        builder.Property(m => m.Key).IsRequired().HasMaxLength(PortalModule.KeyMaxLength);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(PortalModule.NameMaxLength);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(PortalModule.DescriptionMaxLength);

        builder.HasIndex(m => m.Key).IsUnique();
    }
}
