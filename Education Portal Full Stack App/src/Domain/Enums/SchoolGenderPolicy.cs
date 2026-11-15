namespace EducationPortal.Domain.Enums;

/// <summary>
/// Who a school admits. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum SchoolGenderPolicy
{
    BoysOnly = 1,
    GirlsOnly = 2,
    CoEducation = 3,
}
