namespace EducationPortal.Domain.Entities;

/// <summary>
/// A role a school's owner creates: a name, an optional description and the rights it gives per module
/// (<see cref="RolePermission"/>). Owned by the school (<c>SchoolId</c>) and covered by the tenant query filter
/// (SEC-AUTHZ-03). A user can hold several roles and their rights combine; the campuses a user may act on come from the
/// person's campus assignments, not from the role (AGENTS.md section 2, "Roles per school").
/// <para>
/// A system role is one of the built-in roles every school starts with (Principal, Teacher, Student, Parent). Its
/// description and rights can be changed, but it keeps its name and cannot be deleted, so the fixed rules that name it
/// keep working.
/// </para>
/// </summary>
public class SchoolRole : BaseAuditableEntity, ITenantOwned
{
    public const int NameMaxLength = 100;

    public const int DescriptionMaxLength = 500;

    private SchoolRole()
    {
        // For EF Core.
        Name = string.Empty;
    }

    private SchoolRole(int schoolId, string name, string? description, bool isSystem) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        SchoolId = schoolId;
        Name = CheckName(name);
        Describe(description);
        IsSystem = isSystem;
    }

    /// <summary>A role the school's owner adds.</summary>
    public static SchoolRole Create(int schoolId, string name, string? description = null) => new(schoolId, name, description, isSystem: false);

    /// <summary>A built-in role every school starts with: it keeps its name and cannot be deleted.</summary>
    public static SchoolRole CreateSystem(int schoolId, string name, string? description = null) => new(schoolId, name, description, isSystem: true);

    public int SchoolId { get; private set; }

    public string Name { get; private set; }

    /// <summary>What the role is for, shown to the school's administrators. <c>null</c> when none is given.</summary>
    public string? Description { get; private set; }

    /// <summary>True for a built-in role: it keeps its name and cannot be deleted.</summary>
    public bool IsSystem { get; private set; }

    /// <summary>Gives the role a new name. A system role keeps its name; giving it the same name again is allowed.</summary>
    public void Rename(string name)
    {
        var checkedName = CheckName(name);
        if (IsSystem && checkedName != Name)
        {
            throw new InvalidOperationException("A built-in role keeps its name.");
        }

        Name = checkedName;
    }

    /// <summary>Sets the description, trimmed; a blank one is stored as <c>null</c>. At most 500 characters.</summary>
    public void Describe(string? description)
    {
        var trimmed = description?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            Description = null;
            return;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, DescriptionMaxLength, nameof(description));
        Description = trimmed;
    }

    /// <summary>Throws when the role may not be deleted (a system role).</summary>
    public void EnsureCanBeDeleted()
    {
        if (IsSystem)
        {
            throw new InvalidOperationException("A built-in role cannot be deleted.");
        }
    }

    private static string CheckName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        return name.Trim();
    }
}
