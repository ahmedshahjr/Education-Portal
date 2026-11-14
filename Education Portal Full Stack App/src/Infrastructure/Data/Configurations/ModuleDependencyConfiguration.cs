using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class ModuleDependencyConfiguration : IEntityTypeConfiguration<ModuleDependency>
{
    public void Configure(EntityTypeBuilder<ModuleDependency> builder)
    {
        // Modules are soft-deleted, never removed, so a dependency never loses its modules.
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(d => d.ModuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(d => d.RequiresModuleId).OnDelete(DeleteBehavior.Restrict);

        // One row per pair, and never a module requiring itself. Longer loops are refused by the use case
        // (ModuleDependencyGraph.WouldCreateCycle).
        builder.HasIndex(d => new { d.ModuleId, d.RequiresModuleId }).IsUnique();
        builder.HasIndex(d => d.RequiresModuleId);
        builder.ToTable(t => t.HasCheckConstraint("CK_ModuleDependencies_NotSelf", "\"ModuleId\" <> \"RequiresModuleId\""));
    }
}
