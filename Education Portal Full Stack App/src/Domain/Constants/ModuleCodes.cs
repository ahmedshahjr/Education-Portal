namespace EducationPortal.Domain.Constants;

/// <summary>
/// The codes of the modules the code knows about (AGENTS.md section 2). A module's use cases name it by code in
/// <c>[Authorize(Module = ModuleCodes.Students)]</c>. Codes are stored in the database and sent over the API, so never
/// rename one. Add a new module here and to <see cref="Entities.ModuleCatalog"/>.
/// </summary>
public static class ModuleCodes
{
    // Core: always included in every subscription, never sold on their own.
    public const string School = "SCHOOL";
    public const string Branch = "BRANCH";
    public const string AcademicSetup = "ACADEMIC_SETUP";
    public const string UsersRoles = "USERS_ROLES";
    public const string Students = "STUDENTS";
    public const string Staff = "STAFF";

    // Paid: sold one by one or in plans.
    public const string Admissions = "ADMISSIONS";
    public const string Attendance = "ATTENDANCE";
    public const string Fees = "FEES";
    public const string ParentCommunication = "PARENT_COMMUNICATION";
    public const string Exams = "EXAMS";
    public const string Timetable = "TIMETABLE";
    public const string HrPayroll = "HR_PAYROLL";
    public const string Transport = "TRANSPORT";
    public const string Library = "LIBRARY";
    public const string Lms = "LMS";
    public const string Hostel = "HOSTEL";
}
