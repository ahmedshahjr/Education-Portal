namespace EducationPortal.Domain.Entities;

/// <summary>
/// A city or town in a <see cref="State"/>. Platform reference data for address dropdowns, not owned by a school, so it
/// has no <c>SchoolId</c>. A city is identified by its name within its state. A custom row (<see cref="IsCustom"/>) was
/// added by an administrator for a town the dataset lacks, and the import never changes it.
/// </summary>
public class City : BaseAuditableEntity
{
    public const int NameMaxLength = 100;
    public const int NativeNameMaxLength = 200;

    private City()
    {
        // For EF Core.
        Name = string.Empty;
    }

    /// <summary>A city from the location dataset.</summary>
    public static City FromDataset(int stateId, string name, decimal? latitude, decimal? longitude)
    {
        var city = new City { StateId = stateId };
        city.SetName(name);
        city.UpdateFromDataset(latitude, longitude);
        return city;
    }

    /// <summary>A town an administrator adds because the dataset lacks it. The import leaves it alone.</summary>
    public static City CreateCustom(int stateId, string name, string? nativeName, decimal? latitude, decimal? longitude)
    {
        var city = new City { StateId = stateId, IsCustom = true };
        city.SetName(name);
        city.SetNativeName(nativeName);
        city.SetCoordinates(latitude, longitude);
        return city;
    }

    public int StateId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The name in the country's own language and script, if known. The dataset has none for cities yet.</summary>
    public string? NativeName { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    /// <summary>Inactive cities are left out of the dropdowns.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Added by an administrator rather than by the dataset; the import never changes it.</summary>
    public bool IsCustom { get; private set; }

    /// <summary>Applies the dataset's coordinates. Refused for a custom city, so a data update can never overwrite one.</summary>
    public void UpdateFromDataset(decimal? latitude, decimal? longitude)
    {
        if (IsCustom)
        {
            throw new InvalidOperationException("A custom city is never changed by the location data import.");
        }

        SetCoordinates(latitude, longitude);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        Name = name.Trim();
    }

    private void SetNativeName(string? nativeName)
    {
        var native = string.IsNullOrWhiteSpace(nativeName) ? null : nativeName.Trim();
        if (native is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(native.Length, NativeNameMaxLength, nameof(nativeName));
        }

        NativeName = native;
    }

    private void SetCoordinates(decimal? latitude, decimal? longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "A latitude is between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "A longitude is between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
