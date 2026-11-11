namespace EducationPortal.Domain.Constants;

/// <summary>
/// The stable keys of the portal's modules (AGENTS.md section 2). Use cases name their module by key in
/// <c>[Authorize(Module = ModuleKeys.Students)]</c>. Keys are stored in the database, so never rename one; add new
/// modules to <see cref="Entities.ModuleCatalog"/> as well.
/// </summary>
public static class ModuleKeys
{
    public const string Branches = nameof(Branches);
    public const string UsersAndRoles = nameof(UsersAndRoles);
    public const string Students = nameof(Students);
    public const string Parents = nameof(Parents);
    public const string Teachers = nameof(Teachers);
    public const string Courses = nameof(Courses);
    public const string Enrolments = nameof(Enrolments);
    public const string Grades = nameof(Grades);
    public const string Dashboard = nameof(Dashboard);
}
