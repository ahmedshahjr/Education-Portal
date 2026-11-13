namespace EducationPortal.Domain.Entities;

/// <summary>
/// A role a school's owner creates: a name and the rights it gives per module (<see cref="RolePermission"/>). Owned by
/// the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). Assigning roles to users comes
/// later; this is the model the school owner edits (AGENTS.md section 2, "Roles per school").
/// </summary>
public class SchoolRole : BaseAuditableEntity
{
    public const int NameMaxLength = 100;

    private SchoolRole()
    {
        // For EF Core.
        Name = string.Empty;
    }

    private SchoolRole(int schoolId, string name) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        SchoolId = schoolId;
        Rename(name);
    }

    public static SchoolRole Create(int schoolId, string name) => new(schoolId, name);

    public int SchoolId { get; private set; }

    public string Name { get; private set; }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
    }
}
