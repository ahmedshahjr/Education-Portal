namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a gallery photo shows. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum PhotoKind
{
    Campus = 1,
    Classroom = 2,
    Facility = 3,
    Event = 4,
}
