using EducationPortal.Domain.Constants;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The modules the code knows about, with the default/custom split they start with. Seeding adds a missing module from
/// here; after that the master tenant decides which modules are default (<see cref="PortalModule.SetDefault"/>).
/// Add a new module here and in <see cref="ModuleKeys"/>, and to the module table in AGENTS.md section 2.
/// </summary>
public static class ModuleCatalog
{
    public static IReadOnlyList<PortalModule> CreateModules() =>
    [
        PortalModule.Create(ModuleKeys.Branches, "Branches", "Create and edit the school's branches.", isDefault: true),
        PortalModule.Create(ModuleKeys.UsersAndRoles, "Users & Roles", "Staff accounts, roles with rights per module, branch assignment.", isDefault: true),
        PortalModule.Create(ModuleKeys.Students, "Students", "Student records, branch membership, link to parents.", isDefault: true),
        PortalModule.Create(ModuleKeys.Teachers, "Teachers", "Teacher records, branch membership.", isDefault: true),
        PortalModule.Create(ModuleKeys.Courses, "Courses", "Courses per branch, assigned teachers.", isDefault: true),
        PortalModule.Create(ModuleKeys.Enrolments, "Enrolments", "Enrolling students in courses.", isDefault: true),
        PortalModule.Create(ModuleKeys.Grades, "Grades", "Assessments and scores per enrolment.", isDefault: true),
        PortalModule.Create(ModuleKeys.Parents, "Parents", "Parent records, link to children.", isDefault: false),
        PortalModule.Create(ModuleKeys.Dashboard, "Dashboard", "School and branch overview for owners and staff.", isDefault: false),
    ];
}
