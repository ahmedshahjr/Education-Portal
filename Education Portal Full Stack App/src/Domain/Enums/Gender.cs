namespace EducationPortal.Domain.Enums;

/// <summary>
/// A student's gender, as the admission form asks. Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member.
/// </summary>
public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
}
