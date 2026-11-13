namespace EducationPortal.Domain.Enums;

/// <summary>
/// The region where a school's data is kept. Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member.
/// </summary>
public enum DataRegion
{
    SouthAsia = 1,
    MiddleEast = 2,
    NorthAmerica = 3,
}
