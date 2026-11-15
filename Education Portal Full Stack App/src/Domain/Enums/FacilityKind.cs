namespace EducationPortal.Domain.Enums;

/// <summary>
/// A facility a school can list on its public page. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum FacilityKind
{
    Transport = 1,
    Hostel = 2,
    Library = 3,
    ScienceLab = 4,
    ComputerLab = 5,
    SportsGround = 6,
    Cafeteria = 7,
    PrayerArea = 8,
    MedicalRoom = 9,
    Cctv = 10,
    AirConditioning = 11,
    Playground = 12,
    Auditorium = 13,
    SwimmingPool = 14,
}
