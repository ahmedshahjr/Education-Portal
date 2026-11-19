using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Localization;
using EducationPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(u => u.SchoolId);

        // A person has at most one account in their school (accounts without a person are not indexed).
        builder.HasIndex(u => new { u.SchoolId, u.UserDetailsId })
            .IsUnique()
            .HasFilter("\"UserDetailsId\" IS NOT NULL");

        builder.Property(u => u.PreferredLanguage).HasMaxLength(Language.CodeLength);

        builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
