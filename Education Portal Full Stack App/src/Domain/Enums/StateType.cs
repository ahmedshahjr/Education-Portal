namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a country calls its first-level divisions, as named by the location dataset. Stored as its number and sent over
/// the API by name (ENG-ENUM-01..03): never change or reuse a number. Add a member when the dataset brings a new type;
/// the import refuses a type it does not know.
/// </summary>
public enum StateType
{
    State = 1,
    Province = 2,
    Prefecture = 3,
    Emirate = 4,
    Governorate = 5,
    Territory = 6,
    Region = 7,
    Municipality = 8,
    District = 9,
    AdministeredArea = 10,
    FederalCapitalTerritory = 11,
    OutlyingArea = 12,
}
