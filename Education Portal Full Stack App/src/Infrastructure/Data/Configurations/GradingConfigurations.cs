using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class GradingScaleConfiguration : IEntityTypeConfiguration<GradingScale>
{
    public void Configure(EntityTypeBuilder<GradingScale> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(GradingScale.NameMaxLength);
        builder.HasMany(s => s.Bands).WithOne().HasForeignKey(b => b.GradingScaleId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Bands).HasField("_bands").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(s => new { s.SchoolId, s.Name }).IsUnique();
    }
}

public class GradingBandConfiguration : IEntityTypeConfiguration<GradingBand>
{
    public void Configure(EntityTypeBuilder<GradingBand> builder)
    {
        builder.Property(b => b.MinPercent).HasPrecision(5, 2);
        builder.Property(b => b.MaxPercent).HasPrecision(5, 2);
        builder.Property(b => b.Points).HasPrecision(5, 2);
        builder.Property(b => b.Letter).IsRequired().HasMaxLength(5);
        builder.HasIndex(b => new { b.SchoolId, b.GradingScaleId });
    }
}
