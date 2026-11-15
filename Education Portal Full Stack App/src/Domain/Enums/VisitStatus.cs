namespace EducationPortal.Domain.Enums;

/// <summary>
/// Where a campus visit is. Completed, Cancelled and NoShow are final. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum VisitStatus
{
    Requested = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5,
}
