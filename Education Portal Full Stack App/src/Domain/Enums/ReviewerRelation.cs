namespace EducationPortal.Domain.Enums;

/// <summary>
/// How the reviewer knows the school. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum ReviewerRelation
{
    Parent = 1,
    Student = 2,
    Alumni = 3,
    Other = 4,
}
