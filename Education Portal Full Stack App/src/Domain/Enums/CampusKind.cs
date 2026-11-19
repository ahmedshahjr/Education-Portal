namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a campus is for. <see cref="Main"/> is the school's registration campus: every school has exactly one, created
/// with the school. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum CampusKind
{
    Main = 1,
    Other = 2,
}
