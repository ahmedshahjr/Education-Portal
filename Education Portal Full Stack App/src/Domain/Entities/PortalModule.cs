namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module of the portal, such as Students or Grades (AGENTS.md section 2). Modules are platform reference data that only
/// the master tenant changes, so they have no <c>SchoolId</c>.
/// <list type="bullet">
/// <item><b>Core</b> modules (Branches, Users &amp; Roles) are always on for every school and cannot be switched off.</item>
/// <item><b>Default</b> modules are switched on for every school automatically. The master tenant may still switch one
/// off for a single school (decided 2026-11-11).</item>
/// <item><b>Custom</b> modules (not default) stay off until the master tenant switches them on for a school.</item>
/// </list>
/// A school's own choice is a <see cref="SchoolModule"/>; without one, the module's default applies.
/// </summary>
public class PortalModule : BaseAuditableEntity
{
    public const int KeyMaxLength = 50;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 300;

    private PortalModule()
    {
        // For EF Core.
        Key = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    private PortalModule(string key, string name, string description, bool isCore, bool isDefault) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(key.Length, KeyMaxLength, nameof(key));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentNullException.ThrowIfNull(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Trim().Length, DescriptionMaxLength, nameof(description));

        Key = key;
        Name = name.Trim();
        Description = description.Trim();
        IsCore = isCore;
        IsDefault = isCore || isDefault;
    }

    public static PortalModule Create(string key, string name, string description, bool isCore, bool isDefault) =>
        new(key, name, description, isCore, isDefault);

    /// <summary>The stable key from <see cref="Constants.ModuleKeys"/>. Set once.</summary>
    public string Key { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    /// <summary>True for modules every school always has. Set at creation and never changed.</summary>
    public bool IsCore { get; private set; }

    /// <summary>True when the module is switched on for every school that has no choice of its own.</summary>
    public bool IsDefault { get; private set; }

    /// <summary>Makes the module default (on for every school unless switched off) or custom (off unless switched on).</summary>
    public void SetDefault(bool isDefault)
    {
        if (IsCore && !isDefault)
        {
            throw new InvalidOperationException("A core module is always on, so it is always a default module.");
        }

        IsDefault = isDefault;
    }

    /// <summary>Whether the module is on for a school, given that school's own choice (or <c>null</c> when it has none).</summary>
    public bool IsEnabledFor(bool? schoolChoice) => IsEnabled(IsCore, IsDefault, schoolChoice);

    /// <summary>The rule behind <see cref="IsEnabledFor"/>, for queries that project the flags instead of loading modules.</summary>
    public static bool IsEnabled(bool isCore, bool isDefault, bool? schoolChoice) => isCore || (schoolChoice ?? isDefault);
}
