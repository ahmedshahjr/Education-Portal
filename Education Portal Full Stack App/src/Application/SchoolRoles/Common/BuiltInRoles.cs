using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>
/// The four roles every school starts with, one per kind of user (ADR-012 and the people plan). A school may change them
/// and add its own. The rights here are the starting proposal: View lets a role see a module, Create, Edit and Delete let it
/// change what it sees, Approve and Export let it decide on and download what it sees. They are system roles: a school
/// keeps their names and cannot delete them (<see cref="SchoolRole.IsSystem"/>), but it can change their rights.
/// </summary>
public static class BuiltInRoles
{
    public const string Principal = "Principal";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    public const string Parent = "Parent";

    /// <summary>Each built-in role's name and the rights it starts with.</summary>
    public static IReadOnlyList<(string Name, IReadOnlyList<RolePermissionDto> Permissions)> All { get; } =
    [
        (Principal, Enum.GetValues<ModuleCode>().Where(m => m != ModuleCode.None).Select(m => new RolePermissionDto(m, true, true, true, true, true, true)).ToList()),
        (Teacher, Editing(ModuleCode.Students, ModuleCode.Attendance, ModuleCode.Exams, ModuleCode.Timetable, ModuleCode.Lms)),
        (Student, Viewing(ModuleCode.Attendance, ModuleCode.Exams, ModuleCode.Timetable, ModuleCode.Lms)),
        (Parent, Viewing(ModuleCode.Students, ModuleCode.Attendance, ModuleCode.Exams, ModuleCode.Fees, ModuleCode.ParentCommunication, ModuleCode.Timetable)),
    ];

    private static IReadOnlyList<RolePermissionDto> Viewing(params ModuleCode[] modules) =>
        modules.Select(m => new RolePermissionDto(m, true, false, false, false, false, false)).ToList();

    private static IReadOnlyList<RolePermissionDto> Editing(params ModuleCode[] modules) =>
        modules.Select(m => new RolePermissionDto(m, true, true, true, false, false, false)).ToList();
}
