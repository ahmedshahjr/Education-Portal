using EducationPortal.Domain.Constants;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The modules and dependencies the code knows about. Seeding adds any that are missing; after that the master tenant
/// changes them in the master portal. Add a new module here and in <see cref="ModuleCodes"/>, and to the module table
/// in AGENTS.md section 2.
/// </summary>
public static class ModuleCatalog
{
    public static IReadOnlyList<PortalModule> CreateModules() =>
    [
        // Core: part of every subscription.
        PortalModule.Create(ModuleCodes.School, "School", "School profile, settings and the school overview dashboard.", isCore: true, sortOrder: 10),
        PortalModule.Create(ModuleCodes.Branch, "Branches", "Create and edit the school's branches.", isCore: true, sortOrder: 20),
        PortalModule.Create(ModuleCodes.AcademicSetup, "Academic setup", "Academic years, classes, courses and enrolments.", isCore: true, sortOrder: 30),
        PortalModule.Create(ModuleCodes.UsersRoles, "Users & Roles", "Staff accounts, roles with rights per module, branch assignment.", isCore: true, sortOrder: 40),
        PortalModule.Create(ModuleCodes.Students, "Students", "Student records and branch membership.", isCore: true, sortOrder: 50),
        PortalModule.Create(ModuleCodes.Staff, "Staff", "Teacher and staff records and branch membership.", isCore: true, sortOrder: 60),

        // Paid: sold one by one or in plans.
        PortalModule.Create(ModuleCodes.Admissions, "Admissions", "Enquiries, applications and admission decisions.", isCore: false, sortOrder: 100),
        PortalModule.Create(ModuleCodes.Attendance, "Attendance", "Daily and per-lesson attendance for students and staff.", isCore: false, sortOrder: 110),
        PortalModule.Create(ModuleCodes.Fees, "Fees", "Fee structures, student invoices and fee collection.", isCore: false, sortOrder: 120),
        PortalModule.Create(ModuleCodes.ParentCommunication, "Parent communication", "Parent records, links to children and messages to parents.", isCore: false, sortOrder: 130),
        PortalModule.Create(ModuleCodes.Exams, "Exams", "Assessments, scores and report cards.", isCore: false, sortOrder: 140),
        PortalModule.Create(ModuleCodes.Timetable, "Timetable", "Class and teacher timetables.", isCore: false, sortOrder: 150),
        PortalModule.Create(ModuleCodes.HrPayroll, "HR & Payroll", "Staff contracts, leave and payroll.", isCore: false, sortOrder: 160),
        PortalModule.Create(ModuleCodes.Transport, "Transport", "Routes, vehicles and student transport.", isCore: false, sortOrder: 170),
        PortalModule.Create(ModuleCodes.Library, "Library", "Library catalogue, loans and returns.", isCore: false, sortOrder: 180),
        PortalModule.Create(ModuleCodes.Lms, "LMS", "Online lessons, assignments and course material.", isCore: false, sortOrder: 190),
        PortalModule.Create(ModuleCodes.Hostel, "Hostel", "Hostel rooms, allocations and boarders.", isCore: false, sortOrder: 200),
    ];

    /// <summary>
    /// The starting dependencies, as (module, required module) codes. Placeholders for the master tenant to review: core
    /// modules are always included, so a dependency on one only documents the need.
    /// </summary>
    public static IReadOnlyList<(string ModuleCode, string RequiresCode)> Dependencies { get; } =
    [
        (ModuleCodes.Admissions, ModuleCodes.Students),
        (ModuleCodes.Attendance, ModuleCodes.Students),
        (ModuleCodes.Attendance, ModuleCodes.AcademicSetup),
        (ModuleCodes.Fees, ModuleCodes.Students),
        (ModuleCodes.ParentCommunication, ModuleCodes.Students),
        (ModuleCodes.Exams, ModuleCodes.Students),
        (ModuleCodes.Exams, ModuleCodes.AcademicSetup),
        (ModuleCodes.Timetable, ModuleCodes.AcademicSetup),
        (ModuleCodes.Timetable, ModuleCodes.Staff),
        (ModuleCodes.HrPayroll, ModuleCodes.Staff),
        (ModuleCodes.Transport, ModuleCodes.Students),
        (ModuleCodes.Library, ModuleCodes.Students),
        (ModuleCodes.Lms, ModuleCodes.AcademicSetup),
        (ModuleCodes.Hostel, ModuleCodes.Students),
    ];
}
