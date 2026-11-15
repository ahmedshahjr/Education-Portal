using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class PortalModuleConfiguration : IEntityTypeConfiguration<PortalModule>
{
    public void Configure(EntityTypeBuilder<PortalModule> builder)
    {
        builder.ToTable("Modules");

        builder.Property(m => m.Code).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(PortalModule.NameMaxLength);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(PortalModule.DescriptionMaxLength);

        // One live module per code. A deleted module keeps its row (and its rows elsewhere point to it by id), so its code
        // may be used again by a new module.
        builder.HasIndex(m => m.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
