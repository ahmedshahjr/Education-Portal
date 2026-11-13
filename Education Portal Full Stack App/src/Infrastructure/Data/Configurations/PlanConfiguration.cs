using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(p => p.Code).IsRequired().HasMaxLength(Plan.CodeMaxLength);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(Plan.NameMaxLength);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(Plan.DescriptionMaxLength);

        // One live plan per code; a deleted plan keeps its row for subscriptions that point to it by id.
        builder.HasIndex(p => p.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}

public class PlanModuleConfiguration : IEntityTypeConfiguration<PlanModule>
{
    public void Configure(EntityTypeBuilder<PlanModule> builder)
    {
        builder.HasOne<Plan>().WithMany().HasForeignKey(m => m.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(m => m.ModuleId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.PlanId, m.ModuleId }).IsUnique();
        builder.HasIndex(m => m.ModuleId);
    }
}
