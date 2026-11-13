using System.Text.RegularExpressions;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A first-level division of a country: a state, province, prefecture, emirate, governorate or territory. Platform
/// reference data for address dropdowns, not owned by a school, so it has no <c>SchoolId</c>.
/// Rows from the location dataset are kept in step with it by the import. A custom row (<see cref="IsCustom"/>) was added
/// by an administrator and the import never changes it.
/// </summary>
public partial class State : BaseAuditableEntity
{
    public const int NameMaxLength = 100;
    public const int NativeNameMaxLength = 200;
    public const int IsoCodeMaxLength = 10;

    private State()
    {
        // For EF Core.
        Name = string.Empty;
    }

    /// <summary>A state from the location dataset, identified by its ISO 3166-2 code.</summary>
    public static State FromDataset(int countryId, string isoCode, string name, string? nativeName, StateType type)
    {
        var state = new State { CountryId = countryId };
        state.IsoCode = NormaliseIsoCode(isoCode);
        state.UpdateFromDataset(name, nativeName, type);
        return state;
    }

    /// <summary>A state an administrator adds because the dataset lacks it. It has no ISO code and the import leaves it alone.</summary>
    public static State CreateCustom(int countryId, string name, string? nativeName, StateType type)
    {
        var state = new State { CountryId = countryId, IsCustom = true };
        state.SetNames(name, nativeName);
        state.Type = type;
        return state;
    }

    public int CountryId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The name in the country's own language and script, if known.</summary>
    public string? NativeName { get; private set; }

    /// <summary>The ISO 3166-2 code, for example <c>PK-SD</c>. <c>null</c> for a custom state.</summary>
    public string? IsoCode { get; private set; }

    public StateType Type { get; private set; }

    /// <summary>Inactive states are left out of the dropdowns.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Added by an administrator rather than by the dataset; the import never changes it.</summary>
    public bool IsCustom { get; private set; }

    /// <summary>Applies the dataset's values. Refused for a custom state, so a data update can never overwrite one.</summary>
    public void UpdateFromDataset(string name, string? nativeName, StateType type)
    {
        if (IsCustom)
        {
            throw new InvalidOperationException("A custom state is never changed by the location data import.");
        }

        SetNames(name, nativeName);
        Type = type;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetNames(string name, string? nativeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        var native = string.IsNullOrWhiteSpace(nativeName) ? null : nativeName.Trim();
        if (native is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(native.Length, NativeNameMaxLength, nameof(nativeName));
        }

        Name = name.Trim();
        NativeName = native;
    }

    private static string NormaliseIsoCode(string isoCode)
    {
        if (isoCode is null || !IsoCodePattern().IsMatch(isoCode))
        {
            throw new ArgumentException("An ISO 3166-2 code is the country code, a dash and up to three letters or digits, for example PK-SD.", nameof(isoCode));
        }

        return isoCode.ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Za-z]{2}-[A-Za-z0-9]{1,3}$")]
    private static partial Regex IsoCodePattern();
}
