using System.Text.RegularExpressions;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module of the portal that the master tenant sells, such as FEES or EXAMS (AGENTS.md section 2). Modules are
/// platform reference data that only the master tenant changes, so they have no <c>SchoolId</c>.
/// <list type="bullet">
/// <item><see cref="IsCore"/> modules are part of every subscription and are never sold on their own. A core module is
/// always on: it cannot be switched off, made inactive or deleted.</item>
/// <item>Other modules are paid: a school gets one only through its subscription (until subscriptions exist, through
/// the master's per-school switch, <see cref="SchoolModule"/>).</item>
/// <item><c>IsActive</c> is the master's switch for the module itself: an inactive module is off for every school.</item>
/// </list>
/// A module can require others (<see cref="ModuleDependency"/>), for example FEES requires STUDENTS.
/// </summary>
public partial class PortalModule : BaseSoftDeletableEntity
{
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 300;

    private PortalModule()
    {
        // For EF Core.
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    private PortalModule(string code) : this()
    {
        if (!IsValidCode(code))
        {
            throw new ArgumentException("A module code is upper-case letters, digits and underscores, starting with a letter, for example FEES.", nameof(code));
        }

        Code = code;
    }

    public static PortalModule Create(string code, string name, string description, bool isCore, int sortOrder)
    {
        var module = new PortalModule(code);
        module.Update(name, description, isCore, sortOrder);
        return module;
    }

    /// <summary>The stable code, for example <c>FEES</c> (see <see cref="Constants.ModuleCodes"/>). Set once.</summary>
    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    /// <summary>True when the module is part of every subscription and never sold on its own.</summary>
    public bool IsCore { get; private set; }

    /// <summary>Where the module appears in lists and menus, lowest first.</summary>
    public int SortOrder { get; private set; }

    /// <summary>A core module is always on, so it can never be made inactive or deleted.</summary>
    protected override bool CanBeDeactivated => !IsCore;

    /// <summary>Changes the module. Making it core needs it active, because a core module is always on.</summary>
    public void Update(string name, string description, bool isCore, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentNullException.ThrowIfNull(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Trim().Length, DescriptionMaxLength, nameof(description));
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        if (isCore && !IsActive)
        {
            throw new InvalidOperationException("An inactive module cannot be core: core modules are always on.");
        }

        Name = name.Trim();
        Description = description.Trim();
        IsCore = isCore;
        SortOrder = sortOrder;
    }

    /// <summary>True for a code of upper-case letters, digits and underscores that starts with a letter and fits the column.</summary>
    public static bool IsValidCode(string? code) =>
        code is not null && code.Length <= CodeMaxLength && CodePattern().IsMatch(code);

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex CodePattern();
}
