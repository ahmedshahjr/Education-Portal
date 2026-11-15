using System.Text.RegularExpressions;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module of the portal that the master tenant sells, such as FEES or EXAMS (AGENTS.md section 2). Modules are
/// platform reference data that only the master tenant changes, so they have no <c>SchoolId</c>.
/// <list type="bullet">
/// <item><see cref="IsCore"/> modules are part of every subscription and are never sold on their own.</item>
/// <item>A school gets a module through a grant: a registering school gets every <see cref="IsDefaultEnabled"/> module free of
/// charge, and any module through its subscription.</item>
/// <item><c>IsActive</c> is the master's switch for the module itself: an inactive module is off for every school. Every
/// module can be switched off this way, core ones included.</item>
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

    public static PortalModule Create(string code, string name, string description, bool isCore, int sortOrder, bool isDefaultEnabled = false)
    {
        var module = new PortalModule(code);
        module.Update(name, description, isCore, sortOrder, isDefaultEnabled);
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

    /// <summary>
    /// True when a school that registers gets the module automatically, free of charge (<see cref="TenantModule.CreateDefault"/>).
    /// It changes nothing for schools that already exist.
    /// </summary>
    public bool IsDefaultEnabled { get; private set; }

    /// <summary>
    /// Every module can be switched off by the master (<see cref="IsActive"/>), core ones included: an inactive module is off
    /// for every school, and it comes back as it was once reactivated.
    /// </summary>
    protected override bool CanBeDeactivated => true;

    /// <summary>Changes the module.</summary>
    public void Update(string name, string description, bool isCore, int sortOrder, bool isDefaultEnabled = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentNullException.ThrowIfNull(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Trim().Length, DescriptionMaxLength, nameof(description));
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        Name = name.Trim();
        Description = description.Trim();
        IsCore = isCore;
        SortOrder = sortOrder;
        IsDefaultEnabled = isDefaultEnabled;
    }

    /// <summary>True for a code of upper-case letters, digits and underscores that starts with a letter and fits the column.</summary>
    public static bool IsValidCode(string? code) =>
        code is not null && code.Length <= CodeMaxLength && CodePattern().IsMatch(code);

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex CodePattern();
}
