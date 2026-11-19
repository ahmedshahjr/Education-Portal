using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(s => s.SchoolId).OnDelete(DeleteBehavior.Cascade);

        // One admission form per student person; the person is never hard-deleted (soft delete), so keep the form.
        builder.HasOne(s => s.UserDetails).WithOne().HasForeignKey<StudentProfile>(s => s.UserDetailsId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.UserDetailsId).IsUnique();

        // The school's students are listed and filtered by school (ENG-EF-09).
        builder.HasIndex(s => s.SchoolId);

        builder.Property(s => s.FatherName).IsRequired().HasMaxLength(StudentProfile.FatherNameMaxLength);
        builder.Property(s => s.Nationality).IsRequired().HasMaxLength(StudentProfile.NationalityMaxLength);
        builder.Property(s => s.Address).IsRequired().HasMaxLength(StudentProfile.AddressMaxLength);
        builder.Property(s => s.City).IsRequired().HasMaxLength(StudentProfile.CityMaxLength);
        builder.Property(s => s.ClassApplied).IsRequired().HasMaxLength(StudentProfile.ClassMaxLength);
        builder.Property(s => s.PreviousSchool).HasMaxLength(StudentProfile.PreviousSchoolMaxLength);
        builder.Property(s => s.GuardianPhone).IsRequired().HasMaxLength(ContactRules.PhoneMaxLength);
    }
}
