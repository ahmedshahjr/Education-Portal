using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A branch of a school: where day-to-day data lives (AGENTS.md section 2). Owned by the school (<c>SchoolId</c>) and
/// covered by the tenant query filter (SEC-AUTHZ-03). Every school has exactly one <see cref="BranchKind.Main"/> branch,
/// its registration branch, created by <see cref="School.Create"/> in the same save as the school. A <see cref="Code"/>
/// is unique within its school.
/// </summary>
public class Branch : BaseAuditableEntity
{
    public const int NameMaxLength = 200;
    public const int CodeMaxLength = 20;
    public const string MainName = "Main branch";
    public const string MainCode = "MAIN";

    private Branch()
    {
        // For EF Core.
        Name = string.Empty;
        Code = string.Empty;
    }

    private Branch(int schoolId, string name, string code, BranchKind kind)
    {
        SchoolId = schoolId;
        Name = name;
        Code = code;
        Kind = kind;
    }

    /// <summary>Creates a branch that is not the main one. The main branch comes only with its school.</summary>
    public static Branch Create(int schoolId, string name, string code)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        return new Branch(schoolId, name.Trim(), NormalizeCode(code), BranchKind.Other);
    }

    /// <summary>
    /// The school's registration branch. Its <see cref="SchoolId"/> is 0 until the school is saved; EF Core sets it
    /// through the school's branch collection, so the branch is saved with its school or not at all.
    /// </summary>
    internal static Branch CreateMain() => new(0, MainName, MainCode, BranchKind.Main);

    /// <summary>
    /// Trims and upper-cases a branch code, then checks it: 1 to 20 letters, digits or hyphens. Throws otherwise, so a
    /// code is never stored in another form.
    /// </summary>
    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > CodeMaxLength || !normalized.All(ch => ch is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-'))
        {
            throw new ArgumentException("A branch code is up to 20 letters, digits or hyphens.", nameof(code));
        }

        return normalized;
    }

    public int SchoolId { get; private set; }

    public string Name { get; private set; }

    /// <summary>A short code for the branch, unique within its school, for example <c>MAIN</c> or <c>NORTH</c>.</summary>
    public string Code { get; private set; }

    public BranchKind Kind { get; private set; }
}
