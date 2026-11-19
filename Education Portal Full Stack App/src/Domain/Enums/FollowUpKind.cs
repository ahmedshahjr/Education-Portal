namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a staff member did to follow up an enquiry. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum FollowUpKind
{
    Call = 1,
    Email = 2,
    Visit = 3,
    Note = 4,
}
