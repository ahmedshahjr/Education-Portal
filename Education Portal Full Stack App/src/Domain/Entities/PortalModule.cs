namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module of the portal, such as Students or Grades (AGENTS.md section 2). Modules are platform reference data that only
/// the master tenant changes, so they have no <c>SchoolId</c>.
/// <list type="bullet">
/// <item><see cref="Type"/> is <see cref="ModuleType.Default"/> (on for every school unless the master switched it off for
/// one) or <see cref="ModuleType.Custom"/> (off until the master switches it on for a school).</item>
/// <item><c>IsActive</c> is the master's on/off switch for the module itself: an inactive module is off for every
/// school, whatever its type or a school's choice.</item>
/// </list>
/// A school's own choice is a <see cref="SchoolModule"/>; without one, the module's type decides.
/// </summary>
public class PortalModule : BaseSoftDeletableEntity
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

    private PortalModule(string key, string name, string description, ModuleType type) : this()
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
        SetType(type);
    }

    public static PortalModule Create(string key, string name, string description, ModuleType type) =>
        new(key, name, description, type);

    /// <summary>The stable key from <see cref="Constants.ModuleKeys"/>. Set once.</summary>
    public string Key { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    /// <summary>Default (on for every school unless switched off for one) or custom (off unless switched on for a school).</summary>
    public ModuleType Type { get; private set; }

    public void SetType(ModuleType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown module type.");
        }

        Type = type;
    }

    /// <summary>Whether the module is on for a school, given that school's own choice (or <c>null</c> when it has none).</summary>
    public bool IsEnabledFor(bool? schoolChoice) => IsEnabled(IsActive, Type, schoolChoice);

    /// <summary>The rule behind <see cref="IsEnabledFor"/>, for queries that project the fields instead of loading modules.</summary>
    public static bool IsEnabled(bool isActive, ModuleType type, bool? schoolChoice) =>
        isActive && (schoolChoice ?? type == ModuleType.Default);
}
