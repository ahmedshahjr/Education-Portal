using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.Property(c => c.Code).IsRequired().HasMaxLength(Country.CodeLength).IsFixedLength();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(Country.NameMaxLength);
        builder.Property(c => c.DefaultLanguage).IsRequired().HasMaxLength(Language.CodeLength);
        builder.Property(c => c.TimeZoneId).IsRequired().HasMaxLength(Country.TimeZoneIdMaxLength);
        builder.Property(c => c.CurrencyCode).IsRequired().HasMaxLength(Country.CurrencyCodeLength).IsFixedLength();

        // A PostgreSQL text[] column; the entity only changes it through Update (ENG-ENTITY).
        builder.PrimitiveCollection(c => c.Languages)
            .HasField("_languages")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired()
            .ElementType(element => element.HasMaxLength(Language.CodeLength));

        builder.HasIndex(c => c.Code).IsUnique();
    }
}
