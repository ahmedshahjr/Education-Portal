namespace EducationPortal.Domain.Enums;

/// <summary>
/// A board or curriculum a school teaches. A closed set: a new one is a code change. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum CurriculumKind
{
    Matric = 1,
    Intermediate = 2,
    FederalBoard = 3,
    Cambridge = 4,
    InternationalBaccalaureate = 5,
    AmericanDiploma = 6,
    Montessori = 7,
    Religious = 8,
}
