using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(City.NameMaxLength);
        builder.Property(c => c.NativeName).HasMaxLength(City.NativeNameMaxLength);

        // Enough for the dataset's eight decimal places (about 1 mm).
        builder.Property(c => c.Latitude).HasPrecision(10, 8);
        builder.Property(c => c.Longitude).HasPrecision(11, 8);

        builder.HasOne<State>()
            .WithMany()
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        // A city is identified by its name within its state (the import matches on it). Also the foreign key index.
        builder.HasIndex(c => new { c.StateId, c.Name }).IsUnique();
    }
}
