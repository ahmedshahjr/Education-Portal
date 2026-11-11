using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolModuleConfiguration : IEntityTypeConfiguration<SchoolModule>
{
    public void Configure(EntityTypeBuilder<SchoolModule> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(m => m.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(m => m.ModuleId).OnDelete(DeleteBehavior.Cascade);

        // One choice per school and module.
        builder.HasIndex(m => new { m.SchoolId, m.ModuleId }).IsUnique();
    }
}
