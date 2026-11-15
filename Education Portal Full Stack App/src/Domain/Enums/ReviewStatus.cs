namespace EducationPortal.Domain.Enums;

/// <summary>
/// Whether a review is shown on the school's page. Only Approved reviews are public. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum ReviewStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}
