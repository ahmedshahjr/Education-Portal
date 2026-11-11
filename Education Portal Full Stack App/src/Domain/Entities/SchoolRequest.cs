namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school applies to join the platform from the public "request a school" form (decided 2026-11-10). A request creates
/// no account and no school. Only the master tenant approves or rejects it, and approval is what creates the school.
/// </summary>
public class SchoolRequest : BaseAuditableEntity
{
    public const int SchoolNameMaxLength = School.NameMaxLength;
    public const int ContactNameMaxLength = 100;
    public const int ContactEmailMaxLength = 254;

    private SchoolRequest()
    {
        // For EF Core.
        SchoolName = string.Empty;
        ContactName = string.Empty;
        ContactEmail = string.Empty;
    }

    private SchoolRequest(string schoolName, string contactName, string contactEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schoolName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactEmail);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(schoolName.Trim().Length, SchoolNameMaxLength, nameof(schoolName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(contactName.Trim().Length, ContactNameMaxLength, nameof(contactName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(contactEmail.Trim().Length, ContactEmailMaxLength, nameof(contactEmail));

        var email = contactEmail.Trim();
        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
        {
            throw new ArgumentException("The contact email must be an email address.", nameof(contactEmail));
        }

        SchoolName = schoolName.Trim();
        ContactName = contactName.Trim();
        ContactEmail = email;
        Status = SchoolRequestStatus.Pending;
    }

    /// <summary>The domain is not part of the request: the master tenant sets it when it approves the school.</summary>
    public static SchoolRequest Submit(string schoolName, string contactName, string contactEmail) =>
        new(schoolName, contactName, contactEmail);

    public string SchoolName { get; private set; }

    public string ContactName { get; private set; }

    /// <summary>Personal data of the requester. Only master tenant administrators read it (SEC-PII-03).</summary>
    public string ContactEmail { get; private set; }

    public SchoolRequestStatus Status { get; private set; }

    public void Approve()
    {
        EnsurePending();
        Status = SchoolRequestStatus.Approved;
    }

    public void Reject()
    {
        EnsurePending();
        Status = SchoolRequestStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != SchoolRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending school request can be decided.");
        }
    }
}
