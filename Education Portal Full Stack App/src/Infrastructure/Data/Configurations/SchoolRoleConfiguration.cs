using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolRoleConfiguration : IEntityTypeConfiguration<SchoolRole>
{
    public void Configure(EntityTypeBuilder<SchoolRole> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(r => r.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(SchoolRole.NameMaxLength);
        builder.Property(r => r.Description).HasMaxLength(SchoolRole.DescriptionMaxLength);

        // A school's role names are unique.
        builder.HasIndex(r => new { r.SchoolId, r.Name }).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(p => p.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SchoolRole>().WithMany().HasForeignKey(p => p.SchoolRoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(p => p.ModuleId).OnDelete(DeleteBehavior.Restrict);

        // A deleted campus takes the rights given on it with it.
        builder.HasOne<Campus>().WithMany().HasForeignKey(p => p.CampusId).OnDelete(DeleteBehavior.Cascade);

        // One row per role, module and campus (ADR-022).
        builder.HasIndex(p => new { p.SchoolRoleId, p.ModuleId, p.CampusId }).IsUnique();
        builder.HasIndex(p => p.SchoolId);
        builder.HasIndex(p => p.ModuleId);
        builder.HasIndex(p => p.CampusId);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_RolePermissions_NeedView",
            "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\" OR \"CanApprove\" OR \"CanExport\" OR \"CanViewHistory\")"));
    }
}
