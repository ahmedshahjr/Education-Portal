using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(Course.NameMaxLength);
        builder.HasOne<ClassOffering>().WithMany().HasForeignKey(c => c.ClassOfferingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(c => c.TeacherUserDetailsId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Campus>().WithMany().HasForeignKey(c => c.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.SchoolId, c.TeacherUserDetailsId });
        builder.HasIndex(c => new { c.SchoolId, c.ClassOfferingId });
    }
}

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(e => e.StudentUserDetailsId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ClassOffering>().WithMany().HasForeignKey(e => e.ClassOfferingId).OnDelete(DeleteBehavior.Restrict);

        // A student is enrolled in a class once.
        builder.HasIndex(e => new { e.SchoolId, e.StudentUserDetailsId, e.ClassOfferingId }).IsUnique();
    }
}

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(Assessment.NameMaxLength);
        builder.Property(a => a.Weight).HasPrecision(5, 2);
        builder.Property(a => a.MaximumScore).HasPrecision(7, 2);
        builder.HasOne<Course>().WithMany().HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(a => new { a.SchoolId, a.CourseId });
    }
}

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.Property(s => s.Value).HasPrecision(7, 2);
        builder.HasOne<Assessment>().WithMany().HasForeignKey(s => s.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Enrolment>().WithMany().HasForeignKey(s => s.EnrolmentId).OnDelete(DeleteBehavior.Restrict);

        // One score per student and assessment.
        builder.HasIndex(s => new { s.AssessmentId, s.EnrolmentId }).IsUnique();
    }
}

public class TimetableSlotConfiguration : IEntityTypeConfiguration<TimetableSlot>
{
    public void Configure(EntityTypeBuilder<TimetableSlot> builder)
    {
        builder.Property(t => t.Room).HasMaxLength(TimetableSlot.RoomMaxLength);
        builder.HasOne<Course>().WithMany().HasForeignKey(t => t.CourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(t => t.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(t => new { t.SchoolId, t.CourseId });
        builder.HasIndex(t => new { t.SchoolId, t.Day });
    }
}
