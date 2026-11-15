using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// One thing staff did about an enquiry (the table <c>EnquiryFollowUps</c>): a call, an email, a visit or a note, with when
/// it happened. They are the record of an offline or in-person contact. Owned by the school and removed with the enquiry.
/// </summary>
public class EnquiryFollowUp : BaseAuditableEntity, ITenantOwned
{
    public const int NoteMaxLength = 1000;

    private EnquiryFollowUp()
    {
        // For EF Core.
        Note = string.Empty;
        Enquiry = null!;
    }

    public static EnquiryFollowUp Create(AdmissionEnquiry enquiry, FollowUpKind kind, string note, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(enquiry);
        ArgumentException.ThrowIfNullOrWhiteSpace(note);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        var trimmed = note.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, NoteMaxLength, nameof(note));

        return new EnquiryFollowUp { SchoolId = enquiry.SchoolId, Enquiry = enquiry, Kind = kind, Note = trimmed, OccurredAt = occurredAt };
    }

    public int SchoolId { get; private set; }

    public int EnquiryId { get; private set; }

    public AdmissionEnquiry Enquiry { get; private set; }

    public FollowUpKind Kind { get; private set; }

    public string Note { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
