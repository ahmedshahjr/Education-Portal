using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A tenant. Every school is reached on its own domain (AGENTS.md section 2), and the domain is how a request finds its
/// school before anyone signs in. Exactly one school is the master tenant, the platform owner: it is created once by
/// <see cref="CreateMaster"/>, no method can change <see cref="IsMaster"/>, and a unique index allows only one.
/// </summary>
public class School : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 200;
    public const int DomainMaxLength = 253;

    private School()
    {
        // For EF Core.
        Name = string.Empty;
        Domain = string.Empty;
    }

    private School(string name, string domain, bool isMaster)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
        Domain = NormalizeDomain(domain);
        IsMaster = isMaster;
        ThemePreset = ThemePreset.Cyan;
    }

    /// <summary>Creates the master tenant. Only seeding calls this; there is no use case for it.</summary>
    public static School CreateMaster(string name, string domain) => new(name, domain, isMaster: true);

    public static School Create(string name, string domain) => new(name, domain, isMaster: false);

    /// <summary>The master tenant can never be switched off or deleted: the platform needs one.</summary>
    protected override bool CanBeDeactivated => !IsMaster;

    public string Name { get; private set; }

    /// <summary>The host name the school is reached on, in lower case without a port, for example <c>school1.com</c>.</summary>
    public string Domain { get; private set; }

    /// <summary>True for the one master tenant. Set at creation and never changed.</summary>
    public bool IsMaster { get; private set; }

    /// <summary>The accent colour of the school's UI.</summary>
    public ThemePreset ThemePreset { get; private set; }

    /// <summary>The background colour of the sidebar, or <c>null</c> for the theme's default.</summary>
    public HexColor? SidebarColor { get; private set; }

    /// <summary>
    /// The country whose languages, time zone and currency the school uses, or <c>null</c> for the platform defaults
    /// (English only, UTC). Only the master tenant assigns it.
    /// </summary>
    public int? CountryId { get; private set; }

    public void AssignCountry(int? countryId)
    {
        if (countryId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(countryId), countryId, "A country id is positive.");
        }

        CountryId = countryId;
    }

    public void UpdateBranding(ThemePreset themePreset, HexColor? sidebarColor)
    {
        if (!Enum.IsDefined(themePreset))
        {
            throw new ArgumentOutOfRangeException(nameof(themePreset), themePreset, "Unknown theme preset.");
        }

        ThemePreset = themePreset;
        SidebarColor = sidebarColor;
    }

    /// <summary>
    /// Lower-cases a host name and removes a trailing dot. Throws when it is not a plain host name (letters, digits,
    /// dots and hyphens), so a port, path or scheme can never be stored as a domain.
    /// </summary>
    public static string NormalizeDomain(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var normalized = domain.Trim().TrimEnd('.').ToLowerInvariant();

        if (!IsValidDomain(normalized))
        {
            throw new ArgumentException("A domain must be a host name such as school1.com.", nameof(domain));
        }

        return normalized;
    }

    private static bool IsValidDomain(string domain) =>
        domain.Length is > 0 and <= DomainMaxLength
        && domain.Split('.').All(label =>
            label.Length is > 0 and <= 63
            && !label.StartsWith('-')
            && !label.EndsWith('-')
            && label.All(ch => ch is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'));
}
