using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.Property(s => s.TaxRateOverride).HasPrecision(5, 2);
        builder.ToTable(t => t.HasCheckConstraint("CK_Schools_TaxRateOverrideRange", "\"TaxRateOverride\" IS NULL OR (\"TaxRateOverride\" >= 0 AND \"TaxRateOverride\" <= 100)"));
        builder.Property(s => s.Name).IsRequired().HasMaxLength(School.NameMaxLength);
        builder.Property(s => s.CampusCodePrefix).HasMaxLength(School.CampusCodePrefixMaxLength);
        builder.Property(s => s.Domain).HasMaxLength(School.DomainMaxLength);
        builder.Property(s => s.AccentColor)
            .IsRequired()
            .HasConversion(color => color.Value, value => HexColor.From(value))
            .HasMaxLength(HexColor.Length);
        builder.Property(s => s.SidebarColor)
            .HasConversion(color => color!.Value, value => HexColor.From(value))
            .HasMaxLength(HexColor.Length);

        // A country in use cannot be deleted; the foreign key gets an index (ENG-EF-09).
        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Registration and contact details (SchoolDetails). Lengths match the domain limits.
        builder.Property(s => s.LocalName).HasMaxLength(SchoolDetails.LocalNameMaxLength);
        builder.Property(s => s.RegistrationNumber).HasMaxLength(SchoolDetails.RegistrationNumberMaxLength);
        builder.Property(s => s.RegistrationAuthority).HasMaxLength(SchoolDetails.RegistrationAuthorityMaxLength);
        builder.Property(s => s.TaxNumber).HasMaxLength(SchoolDetails.TaxNumberMaxLength);
        builder.Property(s => s.Address).HasMaxLength(SchoolDetails.AddressMaxLength);
        builder.Property(s => s.Phone).HasMaxLength(ContactRules.PhoneMaxLength);
        builder.Property(s => s.Email).HasMaxLength(ContactRules.EmailMaxLength);
        builder.Property(s => s.Website).HasMaxLength(ContactRules.WebsiteMaxLength);
        builder.Property(s => s.AcademicYearStartMonth).HasDefaultValue(SchoolDetails.DefaultAcademicYearStartMonth);
        builder.HasOne(s => s.City).WithMany().HasForeignKey(s => s.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(t => t.HasCheckConstraint("CK_Schools_AcademicYearStartMonth", "\"AcademicYearStartMonth\" BETWEEN 1 AND 12"));

        // Every school belongs to a tenant (ADR-021). A tenant with schools cannot be deleted.
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.TenantId);

        // One school per domain: the domain is how a request finds its tenant (SEC-AUTHZ-10).
        builder.HasIndex(s => s.Domain).IsUnique();

        // At most one master tenant. Seeding creates it, and no use case can create another or change IsMaster.
        builder.HasIndex(s => s.IsMaster)
            .IsUnique()
            .HasFilter("\"IsMaster\"")
            .HasDatabaseName("IX_Schools_SingleMasterTenant");
    }
}
