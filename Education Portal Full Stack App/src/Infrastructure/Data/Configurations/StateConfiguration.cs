using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(State.NameMaxLength);
        builder.Property(s => s.NativeName).HasMaxLength(State.NativeNameMaxLength);
        builder.Property(s => s.IsoCode).HasMaxLength(State.IsoCodeMaxLength);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // The ISO 3166-2 code is unique across all countries; custom states have none (PostgreSQL allows many NULLs).
        builder.HasIndex(s => s.IsoCode).IsUnique();

        // One state per name in a country, so the dropdown never shows the same name twice. Also the foreign key index.
        builder.HasIndex(s => new { s.CountryId, s.Name }).IsUnique();
    }
}
