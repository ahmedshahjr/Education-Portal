using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The modules and dependencies the code knows about. Seeding adds any that are missing; after that the master tenant
/// changes them in the master portal. Add a new module here and in <see cref="ModuleCode"/>, and to the module table
/// in AGENTS.md section 2.
/// </summary>
public static class ModuleCatalog
{
    public static IReadOnlyList<PortalModule> CreateModules() =>
    [
        // Core: part of every subscription, and on by default for a new school (the master can switch any of them off).
        PortalModule.Create(ModuleCode.School, "School", "School profile, settings and the school overview dashboard.", isCore: true, sortOrder: 10, isDefaultEnabled: true),
        PortalModule.Create(ModuleCode.Campus, "Campuses", "Create and edit the school's campuses.", isCore: true, sortOrder: 20, isDefaultEnabled: true),
        PortalModule.Create(ModuleCode.AcademicSetup, "Academic setup", "Academic years, classes, courses and enrolments.", isCore: true, sortOrder: 30, isDefaultEnabled: true),
        PortalModule.Create(ModuleCode.UsersRoles, "Users & Roles", "Staff accounts, roles with rights per module, campus assignment.", isCore: true, sortOrder: 40, isDefaultEnabled: true),
        PortalModule.Create(ModuleCode.Students, "Students", "Student records and campus membership.", isCore: true, sortOrder: 50, isDefaultEnabled: true),
        PortalModule.Create(ModuleCode.Staff, "Staff", "Teacher and staff records and campus membership.", isCore: true, sortOrder: 60, isDefaultEnabled: true),

        // Paid: sold one by one or in plans.
        PortalModule.Create(ModuleCode.Admissions, "Admissions", "Enquiries, applications and admission decisions.", isCore: false, sortOrder: 100),
        PortalModule.Create(ModuleCode.Attendance, "Attendance", "Daily and per-lesson attendance for students and staff.", isCore: false, sortOrder: 110),
        PortalModule.Create(ModuleCode.Fees, "Fees", "Fee structures, student invoices and fee collection.", isCore: false, sortOrder: 120),
        PortalModule.Create(ModuleCode.ParentCommunication, "Parent communication", "Parent records, links to children and messages to parents.", isCore: false, sortOrder: 130),
        PortalModule.Create(ModuleCode.Exams, "Exams", "Assessments, scores and report cards.", isCore: false, sortOrder: 140),
        PortalModule.Create(ModuleCode.Timetable, "Timetable", "Class and teacher timetables.", isCore: false, sortOrder: 150),
        PortalModule.Create(ModuleCode.HrPayroll, "HR & Payroll", "Staff contracts, leave and payroll.", isCore: false, sortOrder: 160),
        PortalModule.Create(ModuleCode.Transport, "Transport", "Routes, vehicles and student transport.", isCore: false, sortOrder: 170),
        PortalModule.Create(ModuleCode.Library, "Library", "Library catalogue, loans and returns.", isCore: false, sortOrder: 180),
        PortalModule.Create(ModuleCode.Lms, "LMS", "Online lessons, assignments and course material.", isCore: false, sortOrder: 190),
        PortalModule.Create(ModuleCode.Hostel, "Hostel", "Hostel rooms, allocations and boarders.", isCore: false, sortOrder: 200),
    ];

    /// <summary>
    /// The starting dependencies, as (module, required module) codes. Placeholders for the master tenant to review: core
    /// modules are always included, so a dependency on one only documents the need.
    /// </summary>
    public static IReadOnlyList<(ModuleCode Module, ModuleCode Requires)> Dependencies { get; } =
    [
        (ModuleCode.Admissions, ModuleCode.Students),
        (ModuleCode.Attendance, ModuleCode.Students),
        (ModuleCode.Attendance, ModuleCode.AcademicSetup),
        (ModuleCode.Fees, ModuleCode.Students),
        (ModuleCode.ParentCommunication, ModuleCode.Students),
        (ModuleCode.Exams, ModuleCode.Students),
        (ModuleCode.Exams, ModuleCode.AcademicSetup),
        (ModuleCode.Timetable, ModuleCode.AcademicSetup),
        (ModuleCode.Timetable, ModuleCode.Staff),
        (ModuleCode.HrPayroll, ModuleCode.Staff),
        (ModuleCode.Transport, ModuleCode.Students),
        (ModuleCode.Library, ModuleCode.Students),
        (ModuleCode.Lms, ModuleCode.AcademicSetup),
        (ModuleCode.Hostel, ModuleCode.Students),
    ];
}
