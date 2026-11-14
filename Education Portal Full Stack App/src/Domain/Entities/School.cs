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

    /// <summary>The accent colour a school starts with: the theme's default cyan.</summary>
    public static readonly HexColor DefaultAccentColor = HexColor.From("#00a1ff");

    private readonly List<Branch> _branches = [];

    private School()
    {
        // For EF Core.
        Name = string.Empty;
        Domain = string.Empty;
        AccentColor = DefaultAccentColor;
    }

    private School(string name, string domain, bool isMaster)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
        Domain = NormalizeDomain(domain);
        IsMaster = isMaster;
        AccentColor = DefaultAccentColor;

        // Every school has its registration branch from the start, saved with it in the same save.
        _branches.Add(Branch.CreateMain());
    }

    /// <summary>
    /// The school's branches. It always holds exactly one main branch, created with the school; nothing removes it, and
    /// the database allows only one per school.
    /// </summary>
    public IReadOnlyCollection<Branch> Branches => _branches;

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

    /// <summary>The accent colour of the school's UI, for example the primary buttons and links.</summary>
    public HexColor AccentColor { get; private set; }

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

    /// <summary>Where the school's data is kept, or <c>null</c> when not set. Only the master tenant sets it.</summary>
    public DataRegion? DataRegion { get; private set; }

    public void AssignDataRegion(DataRegion? dataRegion)
    {
        if (dataRegion is { } region && !Enum.IsDefined(region))
        {
            throw new ArgumentOutOfRangeException(nameof(dataRegion), dataRegion, "Unknown data region.");
        }

        DataRegion = dataRegion;
    }

    /// <summary>
    /// A tax rate in percent for this school's invoices that replaces its country's price-book rate, or <c>null</c> to use
    /// the country's. Only the master tenant sets it.
    /// </summary>
    public decimal? TaxRateOverride { get; private set; }

    public void SetTaxRateOverride(decimal? taxRate)
    {
        if (taxRate is { } rate && !PriceBook.IsValidTaxRate(rate))
        {
            throw new ArgumentOutOfRangeException(nameof(taxRate), taxRate, "A tax rate is a percentage from 0 to 100 with at most two decimal places.");
        }

        TaxRateOverride = taxRate;
    }

    /// <summary>
    /// Changes the school's name and the host name it is reached on. The master tenant's domain never changes: the platform
    /// is reached on it. Domain uniqueness across schools is checked by the use case, which has the other schools.
    /// </summary>
    public void UpdateProfile(string name, string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        var normalizedDomain = NormalizeDomain(domain);
        if (IsMaster && normalizedDomain != Domain)
        {
            throw new InvalidOperationException("The master tenant's domain cannot be changed.");
        }

        Name = name.Trim();
        Domain = normalizedDomain;
    }

    public void UpdateBranding(HexColor accentColor, HexColor? sidebarColor)
    {
        ArgumentNullException.ThrowIfNull(accentColor);

        AccentColor = accentColor;
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

    /// <summary>True when the lower-case domain is a valid host name (used by request validation too).</summary>
    public static bool IsValidDomain(string domain) =>
        domain.Length is > 0 and <= DomainMaxLength
        && domain.Split('.').All(label =>
            label.Length is > 0 and <= 63
            && !label.StartsWith('-')
            && !label.EndsWith('-')
            && label.All(ch => ch is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'));
}
