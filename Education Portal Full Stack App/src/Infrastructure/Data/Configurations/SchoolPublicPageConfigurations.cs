using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

// The tables behind a school's public page and its admission enquiries. Every one belongs to a school (ITenantOwned), and the
// tenant query filter is applied to all of them in ApplicationDbContext.

public class SchoolProfileConfiguration : IEntityTypeConfiguration<SchoolProfile>
{
    public void Configure(EntityTypeBuilder<SchoolProfile> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(p => p.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.SchoolId).IsUnique(); // one page per school

        builder.Property(p => p.Tagline).IsRequired().HasMaxLength(SchoolProfile.TaglineMaxLength);
        builder.Property(p => p.About).IsRequired().HasMaxLength(SchoolProfile.AboutMaxLength);
        builder.Property(p => p.VisitingHours).HasMaxLength(SchoolProfile.VisitingHoursMaxLength);
        builder.Property(p => p.VirtualTourUrl).HasMaxLength(ContactRules.WebsiteMaxLength);
    }
}

public class SchoolCurriculumConfiguration : IEntityTypeConfiguration<SchoolCurriculum>
{
    public void Configure(EntityTypeBuilder<SchoolCurriculum> builder)
    {
        builder.ToTable("SchoolCurricula");
        builder.HasOne<School>().WithMany().HasForeignKey(c => c.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.SchoolId, c.Kind }).IsUnique();
    }
}

public class SchoolFacilityConfiguration : IEntityTypeConfiguration<SchoolFacility>
{
    public void Configure(EntityTypeBuilder<SchoolFacility> builder)
    {
        builder.ToTable("SchoolFacilities");
        builder.HasOne<School>().WithMany().HasForeignKey(f => f.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(f => new { f.SchoolId, f.Kind }).IsUnique();
        builder.Property(f => f.Description).HasMaxLength(SchoolFacility.DescriptionMaxLength);
    }
}

public class ClassOfferingConfiguration : IEntityTypeConfiguration<ClassOffering>
{
    public void Configure(EntityTypeBuilder<ClassOffering> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(o => o.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(o => o.CampusId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.AcademicYear).IsRequired().HasMaxLength(ClassOffering.AcademicYearMaxLength);
        builder.Property(o => o.CurrencyCode).IsRequired().HasMaxLength(ClassOffering.CurrencyCodeLength).IsFixedLength();
        builder.Property(o => o.MonthlyFee).HasPrecision(18, 2);
        builder.Property(o => o.AdmissionFee).HasPrecision(18, 2);

        // One live offering per class, year and campus; the whole school (no campus) counts as one campus value.
        builder.HasIndex(o => new { o.SchoolId, o.CampusId, o.Level, o.AcademicYear })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(o => o.CampusId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ClassOfferings_FeesNotNegative", "\"MonthlyFee\" >= 0 AND (\"AdmissionFee\" IS NULL OR \"AdmissionFee\" >= 0)");
            t.HasCheckConstraint("CK_ClassOfferings_Seats", "\"SeatsTotal\" >= 0 AND \"SeatsAvailable\" >= 0 AND \"SeatsAvailable\" <= \"SeatsTotal\"");
        });
    }
}

public class SchoolPhotoConfiguration : IEntityTypeConfiguration<SchoolPhoto>
{
    public void Configure(EntityTypeBuilder<SchoolPhoto> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(p => p.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(p => p.CampusId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Caption).HasMaxLength(SchoolPhoto.CaptionMaxLength);
        builder.Property(p => p.ContentType).IsRequired().HasMaxLength(32);
        builder.Property(p => p.Content).IsRequired();
        builder.Property(p => p.Version).IsRequired().HasMaxLength(SchoolPhoto.VersionLength);

        builder.HasIndex(p => new { p.SchoolId, p.SortOrder });
        builder.HasIndex(p => p.CampusId);
        builder.HasIndex(p => new { p.SchoolId, p.WholeSchoolKey }); // the copies of one whole-school photo (ADR-027)
    }
}

public class AdmissionEnquiryConfiguration : IEntityTypeConfiguration<AdmissionEnquiry>
{
    public void Configure(EntityTypeBuilder<AdmissionEnquiry> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(e => e.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(e => e.CampusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(e => e.AssignedToUserDetailsId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.ChildName).IsRequired().HasMaxLength(AdmissionEnquiry.NameMaxLength);
        builder.Property(e => e.ParentName).IsRequired().HasMaxLength(AdmissionEnquiry.NameMaxLength);
        builder.Property(e => e.Phone).IsRequired().HasMaxLength(ContactRules.PhoneMaxLength);
        builder.Property(e => e.Email).HasMaxLength(ContactRules.EmailMaxLength);
        builder.Property(e => e.Message).HasMaxLength(AdmissionEnquiry.MessageMaxLength);

        // The school's inbox: newest first, filtered by status.
        builder.HasIndex(e => new { e.SchoolId, e.Status, e.SubmittedAt });
        builder.HasIndex(e => e.CampusId);
        builder.HasIndex(e => e.AssignedToUserDetailsId);

        // An online enquiry always records the parent's consent.
        builder.ToTable(t => t.HasCheckConstraint("CK_AdmissionEnquiries_OnlineNeedsConsent", "\"Channel\" <> 1 OR \"ConsentGivenAt\" IS NOT NULL"));
    }
}

public class EnquiryFollowUpConfiguration : IEntityTypeConfiguration<EnquiryFollowUp>
{
    public void Configure(EntityTypeBuilder<EnquiryFollowUp> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(f => f.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Enquiry).WithMany().HasForeignKey(f => f.EnquiryId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.Note).IsRequired().HasMaxLength(EnquiryFollowUp.NoteMaxLength);
        builder.HasIndex(f => new { f.EnquiryId, f.OccurredAt });
    }
}

public class CampusVisitConfiguration : IEntityTypeConfiguration<CampusVisit>
{
    public void Configure(EntityTypeBuilder<CampusVisit> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(v => v.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.Enquiry).WithMany().HasForeignKey(v => v.EnquiryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(v => v.CampusId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.Notes).HasMaxLength(CampusVisit.NotesMaxLength);
        builder.HasIndex(v => new { v.SchoolId, v.ScheduledAt });
        builder.HasIndex(v => v.EnquiryId);
        builder.HasIndex(v => v.CampusId);
    }
}

public class SchoolReviewConfiguration : IEntityTypeConfiguration<SchoolReview>
{
    public void Configure(EntityTypeBuilder<SchoolReview> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(r => r.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(r => r.CampusId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.ReviewerName).IsRequired().HasMaxLength(SchoolReview.NameMaxLength);
        builder.Property(r => r.Title).HasMaxLength(SchoolReview.TitleMaxLength);
        builder.Property(r => r.Body).IsRequired().HasMaxLength(SchoolReview.BodyMaxLength);
        builder.Property(r => r.ModeratedBy).HasMaxLength(450);
        builder.Property(r => r.Reply).HasMaxLength(SchoolReview.ReplyMaxLength);

        // The public page lists approved reviews, newest first.
        builder.HasIndex(r => new { r.SchoolId, r.Status, r.SubmittedAt });
        builder.HasIndex(r => r.CampusId);

        builder.ToTable(t => t.HasCheckConstraint("CK_SchoolReviews_Rating", $"\"Rating\" BETWEEN {SchoolReview.MinimumRating} AND {SchoolReview.MaximumRating}"));
    }
}
