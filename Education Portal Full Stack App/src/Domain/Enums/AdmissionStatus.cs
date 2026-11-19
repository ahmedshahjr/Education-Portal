namespace EducationPortal.Domain.Enums;

/// <summary>
/// Whether a class takes new students. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum AdmissionStatus
{
    Open = 1,
    Closed = 2,
    WaitingList = 3,
}
