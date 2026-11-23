using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A parent's interest in a school (the table <c>AdmissionEnquiries</c>). It arrives through the public form
/// (<see cref="EnquiryChannel.Online"/>) or is entered by staff after a call or a visit. The school follows it up
/// (<see cref="EnquiryFollowUp"/>), may book a <see cref="CampusVisit"/>, and moves it through <see cref="EnquiryStatus"/>.
/// </summary>
/// <remarks>
/// Holds personal data of a parent and a child, so it is collected sparingly (names and a phone number, no date of birth),
/// an online enquiry records the parent's consent, and it is deleted for real, not soft-deleted, when the school erases it
/// (student-data-privacy, SEC-PII-06: the retention period is still to be decided).
/// </remarks>
public class AdmissionEnquiry : BaseAuditableEntity, ITenantOwned
{
    public const int NameMaxLength = 100;
    public const int MessageMaxLength = 1000;

    private AdmissionEnquiry()
    {
        // For EF Core.
        ChildName = string.Empty;
        ParentName = string.Empty;
        Phone = string.Empty;
    }

    /// <summary>
    /// Creates a new enquiry. The child's and parent's names and a phone number are required; the email and message are
    /// optional. An online enquiry needs the time the parent gave consent to be contacted.
    /// </summary>
    public static AdmissionEnquiry Create(
        int schoolId,
        int campusId,
        ClassLevel? level,
        EnquiryChannel channel,
        string childName,
        string parentName,
        string phone,
        string? email,
        string? message,
        DateTimeOffset? consentGivenAt,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        if (level is { } known && !Enum.IsDefined(known))
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        if (!Enum.IsDefined(channel))
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        if (channel == EnquiryChannel.Online && consentGivenAt is null)
        {
            throw new ArgumentException("An online enquiry needs the parent's consent to be contacted.", nameof(consentGivenAt));
        }

        var trimmedPhone = Required(phone, ContactRules.PhoneMaxLength, nameof(phone));
        if (!ContactRules.IsValidPhone(trimmedPhone))
        {
            throw new ArgumentException("A phone number has 7 to 15 digits and an optional leading +.", nameof(phone));
        }

        var trimmedEmail = email?.Trim();
        if (!string.IsNullOrEmpty(trimmedEmail) && !ContactRules.IsValidEmail(trimmedEmail))
        {
            throw new ArgumentException("The email address is not valid.", nameof(email));
        }

        var trimmedMessage = message?.Trim();
        if (!string.IsNullOrEmpty(trimmedMessage))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedMessage.Length, MessageMaxLength, nameof(message));
        }

        return new AdmissionEnquiry
        {
            SchoolId = schoolId,
            CampusId = campusId,
            Level = level,
            Channel = channel,
            Status = EnquiryStatus.New,
            ChildName = Required(childName, NameMaxLength, nameof(childName)),
            ParentName = Required(parentName, NameMaxLength, nameof(parentName)),
            Phone = trimmedPhone,
            Email = string.IsNullOrEmpty(trimmedEmail) ? null : trimmedEmail,
            Message = string.IsNullOrEmpty(trimmedMessage) ? null : trimmedMessage,
            ConsentGivenAt = consentGivenAt,
            SubmittedAt = now,
        };
    }

    public int SchoolId { get; private set; }

    /// <summary>The campus the parent asked about, or <c>null</c> when no campus was named.</summary>
    public int CampusId { get; private set; }

    /// <summary>The class the parent asked about, when named.</summary>
    public ClassLevel? Level { get; private set; }

    public EnquiryChannel Channel { get; private set; }

    public EnquiryStatus Status { get; private set; }

    public string ChildName { get; private set; }

    public string ParentName { get; private set; }

    public string Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Message { get; private set; }

    /// <summary>When the parent agreed to be contacted. Required for an online enquiry.</summary>
    public DateTimeOffset? ConsentGivenAt { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }

    /// <summary>The staff member (a <see cref="UserDetails"/> of the same school) who handles the enquiry.</summary>
    public int? AssignedToUserDetailsId { get; private set; }

    public DateTimeOffset? ContactedAt { get; private set; }

    /// <summary>When the enquiry became <see cref="EnquiryStatus.Enrolled"/> or <see cref="EnquiryStatus.Closed"/>.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    public void AssignTo(int userDetailsId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userDetailsId);
        EnsureOpen();
        AssignedToUserDetailsId = userDetailsId;
    }

    /// <summary>
    /// Moves the enquiry to <paramref name="next"/>. The path is New, Contacted, VisitScheduled, Applied, Enrolled, and a school
    /// can skip a step; it cannot go back. An open enquiry can be Closed at any point. Enrolled and Closed are final.
    /// </summary>
    public void ChangeStatus(EnquiryStatus next, DateTimeOffset now)
    {
        if (!Enum.IsDefined(next))
        {
            throw new ArgumentOutOfRangeException(nameof(next));
        }

        EnsureOpen();

        if (next != EnquiryStatus.Closed && next <= Status)
        {
            throw new InvalidOperationException($"An enquiry cannot go from {Status} to {next}.");
        }

        // Any step after New, except closing, means the school has been in touch.
        if (next is not (EnquiryStatus.New or EnquiryStatus.Closed))
        {
            ContactedAt ??= now;
        }

        if (next is EnquiryStatus.Enrolled or EnquiryStatus.Closed)
        {
            ClosedAt = now;
        }

        Status = next;
    }

    private void EnsureOpen()
    {
        if (Status is EnquiryStatus.Enrolled or EnquiryStatus.Closed)
        {
            throw new InvalidOperationException("A finished enquiry cannot be changed.");
        }
    }

    private static string Required(string value, int maxLength, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var trimmed = value.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, maxLength, name);
        return trimmed;
    }
}
