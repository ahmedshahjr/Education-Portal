using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(Tenant.NameMaxLength);

        // Exactly one master tenant (ADR-021). Creation is by seeding only; no use case can add another or change IsMaster.
        builder.HasIndex(t => t.IsMaster)
            .IsUnique()
            .HasFilter("\"IsMaster\"")
            .HasDatabaseName("IX_Tenants_SingleMasterTenant");
    }
}
