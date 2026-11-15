namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a person in a school is for: the campus principal, a teacher, or other staff. Stored as its number and sent over
/// the API by name (ENG-ENUM-01..03): never change or reuse a number, and never rename a member.
/// </summary>
public enum UserType
{
    Principal = 1,
    Teacher = 2,
    Staff = 3,
}
