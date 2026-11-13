namespace EducationPortal.Domain.Entities;

/// <summary>
/// A branch of a school: where day-to-day data lives (AGENTS.md section 2). Minimal for now: a tenant-owned row with a
/// name, so a module can be granted to one branch (<see cref="TenantModule.BranchId"/>). Owned by the school
/// (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03).
/// </summary>
public class Branch : BaseAuditableEntity
{
    public const int NameMaxLength = 200;

    private Branch()
    {
        // For EF Core.
        Name = string.Empty;
    }

    private Branch(int schoolId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        SchoolId = schoolId;
        Name = name.Trim();
    }

    public static Branch Create(int schoolId, string name) => new(schoolId, name);

    public int SchoolId { get; private set; }

    public string Name { get; private set; }
}
