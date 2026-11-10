using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(School.NameMaxLength);
        builder.Property(s => s.Domain).IsRequired().HasMaxLength(School.DomainMaxLength);
        builder.Property(s => s.ThemePreset).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.SidebarColor)
            .HasConversion(color => color!.Value, value => HexColor.From(value))
            .HasMaxLength(HexColor.Length);

        // A country in use cannot be deleted; the foreign key gets an index (ENG-EF-09).
        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One school per domain: the domain is how a request finds its tenant (SEC-AUTHZ-10).
        builder.HasIndex(s => s.Domain).IsUnique();

        // At most one master tenant. Seeding creates it, and no use case can create another or change IsMaster.
        builder.HasIndex(s => s.IsMaster)
            .IsUnique()
            .HasFilter("\"IsMaster\"")
            .HasDatabaseName("IX_Schools_SingleMasterTenant");
    }
}
