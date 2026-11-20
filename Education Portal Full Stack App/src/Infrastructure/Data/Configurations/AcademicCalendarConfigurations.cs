using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

// The school calendar: academic years and their terms, campus shifts, holidays and events. Every one belongs to a school
// (ITenantOwned), and the tenant query filter, combined with "not deleted", is applied to all of them in ApplicationDbContext.
// Uniqueness counts only rows that are not deleted, so a deleted row frees its name.

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(y => y.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(y => y.Name).IsRequired().HasMaxLength(AcademicYear.NameMaxLength);

        // Names are unique within a school. This index also serves the SchoolId foreign key (ENG-EF-09).
        builder.HasIndex(y => new { y.SchoolId, y.Name }).IsUnique().HasFilter("\"IsDeleted\" = false");

        // At most one current year per school.
        builder.HasIndex(y => y.SchoolId)
            .IsUnique()
            .HasFilter("\"IsCurrent\" AND \"IsDeleted\" = false")
            .HasDatabaseName("IX_AcademicYears_SingleCurrentPerSchool");

        builder.ToTable(t => t.HasCheckConstraint("CK_AcademicYears_Dates", "\"EndDate\" >= \"StartDate\""));
    }
}

public class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(t => t.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AcademicYear>().WithMany().HasForeignKey(t => t.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(Term.NameMaxLength);

        // One term per place in a year. This index also serves the AcademicYearId foreign key.
        builder.HasIndex(t => new { t.AcademicYearId, t.Sequence }).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(t => t.SchoolId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Terms_Dates", "\"EndDate\" >= \"StartDate\"");
            t.HasCheckConstraint("CK_Terms_Sequence", $"\"Sequence\" BETWEEN 1 AND {Term.MaxSequence}");
        });
    }
}

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(s => s.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(s => s.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(Shift.NameMaxLength);

        // Names are unique within a campus. This index also serves the CampusId foreign key.
        builder.HasIndex(s => new { s.CampusId, s.Name }).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(s => s.SchoolId);

        builder.ToTable(t => t.HasCheckConstraint("CK_Shifts_Times", "\"EndTime\" > \"StartTime\""));
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(h => h.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(h => h.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(h => h.Title).IsRequired().HasMaxLength(Holiday.TitleMaxLength);

        // The calendar reads a school's holidays by date range.
        builder.HasIndex(h => new { h.SchoolId, h.StartDate });
        builder.HasIndex(h => h.CampusId);

        builder.ToTable(t => t.HasCheckConstraint("CK_Holidays_Dates", "\"EndDate\" >= \"StartDate\""));
    }
}

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(e => e.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(e => e.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(CalendarEvent.TitleMaxLength);
        builder.Property(e => e.Description).HasMaxLength(CalendarEvent.DescriptionMaxLength);

        // The calendar reads a school's events by date range.
        builder.HasIndex(e => new { e.SchoolId, e.EventDate });
        builder.HasIndex(e => e.CampusId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_CalendarEvents_EndNeedsStart", "\"EndTime\" IS NULL OR \"StartTime\" IS NOT NULL");
            t.HasCheckConstraint("CK_CalendarEvents_Times", "\"EndTime\" IS NULL OR \"EndTime\" > \"StartTime\"");
        });
    }
}
