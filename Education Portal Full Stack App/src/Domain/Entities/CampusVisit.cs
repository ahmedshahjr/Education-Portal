using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A parent's visit to a campus (the table <c>CampusVisits</c>), booked from an enquiry. It starts as Requested, the school
/// confirms it, and it ends Completed, Cancelled or as a NoShow. Owned by the school and removed with the enquiry.
/// </summary>
public class CampusVisit : BaseAuditableEntity, ITenantOwned
{
    public const int NotesMaxLength = 500;

    private CampusVisit()
    {
        // For EF Core.
        Enquiry = null!;
    }

    /// <summary>Books a visit that has not happened yet: <paramref name="scheduledAt"/> is after <paramref name="now"/>.</summary>
    public static CampusVisit Request(AdmissionEnquiry enquiry, int campusId, DateTimeOffset scheduledAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(enquiry);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        if (scheduledAt <= now)
        {
            throw new ArgumentOutOfRangeException(nameof(scheduledAt), scheduledAt, "A visit is booked for a time in the future.");
        }

        return new CampusVisit
        {
            SchoolId = enquiry.SchoolId,
            Enquiry = enquiry,
            CampusId = campusId,
            ScheduledAt = scheduledAt,
            Status = VisitStatus.Requested,
        };
    }

    public int SchoolId { get; private set; }

    public int EnquiryId { get; private set; }

    public AdmissionEnquiry Enquiry { get; private set; }

    public int CampusId { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public VisitStatus Status { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>The school confirms a requested visit.</summary>
    public void Confirm()
    {
        Require(VisitStatus.Requested);
        Status = VisitStatus.Confirmed;
    }

    /// <summary>The visit took place. It must have been confirmed.</summary>
    public void Complete(string? notes)
    {
        Require(VisitStatus.Confirmed);
        Status = VisitStatus.Completed;
        Notes = CleanNotes(notes);
    }

    /// <summary>The visit is called off, by the parent or the school. A requested or confirmed visit can be cancelled.</summary>
    public void Cancel(string? notes)
    {
        Require(VisitStatus.Requested, VisitStatus.Confirmed);
        Status = VisitStatus.Cancelled;
        Notes = CleanNotes(notes);
    }

    /// <summary>The parent did not come. The visit must have been confirmed and its time must have passed.</summary>
    public void MarkNoShow(DateTimeOffset now)
    {
        Require(VisitStatus.Confirmed);

        if (now < ScheduledAt)
        {
            throw new InvalidOperationException("A visit is a no-show only after its time has passed.");
        }

        Status = VisitStatus.NoShow;
    }

    private void Require(params VisitStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException($"A visit that is {Status} cannot do this.");
        }
    }

    private static string? CleanNotes(string? notes)
    {
        var trimmed = notes?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, NotesMaxLength, nameof(notes));
        return trimmed;
    }
}
