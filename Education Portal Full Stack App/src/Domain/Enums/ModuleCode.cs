namespace EducationPortal.Domain.Enums;

/// <summary>
/// The modules of the portal (AGENTS.md section 2). A fixed catalogue: the master tenant changes a module's name, price,
/// order and dependencies, but cannot invent a module. A module's use cases name it in
/// <c>[Authorize(Module = ModuleCode.Students)]</c>. Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member. Add a new module here, to <c>ModuleCatalog</c> and to the
/// module table in AGENTS.md.
/// </summary>
public enum ModuleCode
{
    /// <summary>No module: the default of an attribute that names none.</summary>
    None = 0,

    // Core: always included in every subscription, never sold on their own.
    School = 1,
    Campus = 2,
    AcademicSetup = 3,
    UsersRoles = 4,
    Students = 5,
    Staff = 6,

    // Paid: sold one by one or in plans.
    Admissions = 7,
    Attendance = 8,
    Fees = 9,
    ParentCommunication = 10,
    Exams = 11,
    Timetable = 12,
    HrPayroll = 13,
    Transport = 14,
    Library = 15,
    Lms = 16,
    Hostel = 17,
}
