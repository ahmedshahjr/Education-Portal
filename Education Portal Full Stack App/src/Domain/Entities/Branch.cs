using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A campus (the portal calls branches campuses; the class keeps its name): where day-to-day data lives (AGENTS.md section 2).
/// Owned by the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). Every school has exactly one
/// <see cref="BranchKind.Main"/> campus, its registration campus, created by <see cref="School.Create"/> in the same save as
/// the school. A campus has a <see cref="Code"/>: four digits, generated when it is added, unique within its school. The
/// school may show it with a prefix (<see cref="School.CampusDisplayCode"/>).
/// </summary>
public class Branch : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 200;
    public const int CodeLength = 4;
    public const string MainName = "Main campus";

    /// <summary>The main campus is always the first code of a school.</summary>
    public const string MainCode = "0001";

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

    /// <summary>Creates a campus that is not the main one, with the code the use case generated. The main campus comes only with its school.</summary>
    public static Branch Create(int schoolId, string name, string code)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        return new Branch(schoolId, name.Trim(), NormalizeCode(code), BranchKind.Other);
    }

    /// <summary>
    /// The school's registration campus. Its <see cref="SchoolId"/> is 0 until the school is saved; EF Core sets it
    /// through the school's branch collection, so the campus is saved with its school or not at all.
    /// </summary>
    internal static Branch CreateMain() => new(0, MainName, MainCode, BranchKind.Main);

    /// <summary>True when the code is exactly four digits.</summary>
    public static bool IsValidCode(string code) =>
        code.Length == CodeLength && code.All(ch => ch is >= '0' and <= '9');

    /// <summary>Trims the code and checks it is exactly four digits. Throws otherwise, so a code is never stored in another form.</summary>
    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim();
        if (!IsValidCode(normalized))
        {
            throw new ArgumentException("A campus code is four digits.", nameof(code));
        }

        return normalized;
    }

    public int SchoolId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The campus code: four digits, unique within its school, without any school prefix.</summary>
    public string Code { get; private set; }

    public BranchKind Kind { get; private set; }

    /// <summary>The main campus is the school's registration campus: it cannot be deleted or deactivated.</summary>
    protected override bool CanBeDeactivated => Kind != BranchKind.Main;

    /// <summary>Changes the campus's name. The code is generated once and never changes.</summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
    }
}
