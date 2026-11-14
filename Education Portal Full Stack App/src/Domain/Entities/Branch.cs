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

    public const int AddressMaxLength = 300;
    public const int GooglePlaceIdMaxLength = 256;
    public const int GoogleMapsUrlMaxLength = 500;

    /// <summary>A campus has at most this many shifts (Morning and Evening are the usual two).</summary>
    public const int MaxShiftsPerCampus = 4;

    private readonly List<CampusShift> _shifts = [];

    /// <summary>The campus's active shifts. A replaced shift is soft-deleted and is not listed.</summary>
    public IReadOnlyCollection<CampusShift> Shifts => _shifts.AsReadOnly();

    /// <summary>The province (state) the campus is in, from the location data. Null when not set.</summary>
    public int? StateId { get; private set; }

    /// <summary>The city the campus is in, from the location data. Null when not set. Always in <see cref="StateId"/>.</summary>
    public int? CityId { get; private set; }

    public string? AddressLine { get; private set; }

    /// <summary>Latitude in decimal degrees, from -90 to 90. Set together with <see cref="Longitude"/>.</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>Longitude in decimal degrees, from -180 to 180. Set together with <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; private set; }

    /// <summary>The Google place id of the pinned location, when the pin came from a Google place.</summary>
    public string? GooglePlaceId { get; private set; }

    /// <summary>A https link to the pinned location on Google Maps.</summary>
    public string? GoogleMapsUrl { get; private set; }

    /// <summary>
    /// Sets where the campus is. Every part is optional, but a city needs its province, the latitude and longitude come
    /// together, and the Google Maps link must be an https address. Empty text is stored as null.
    /// </summary>
    public void SetLocation(
        int? stateId,
        int? cityId,
        string? addressLine,
        decimal? latitude,
        decimal? longitude,
        string? googlePlaceId,
        string? googleMapsUrl)
    {
        if (cityId is not null && stateId is null)
        {
            throw new ArgumentException("A city needs its province.", nameof(cityId));
        }

        if ((latitude is null) != (longitude is null))
        {
            throw new ArgumentException("Latitude and longitude are given together.", nameof(latitude));
        }

        if (latitude is not null && longitude is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(latitude.Value, -90m, nameof(latitude));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(latitude.Value, 90m, nameof(latitude));
            ArgumentOutOfRangeException.ThrowIfLessThan(longitude.Value, -180m, nameof(longitude));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(longitude.Value, 180m, nameof(longitude));
        }

        var mapsUrl = Trimmed(googleMapsUrl, GoogleMapsUrlMaxLength, nameof(googleMapsUrl));
        if (mapsUrl is not null && !(Uri.TryCreate(mapsUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
        {
            throw new ArgumentException("A Google Maps link is an https address.", nameof(googleMapsUrl));
        }

        StateId = stateId;
        CityId = cityId;
        AddressLine = Trimmed(addressLine, AddressMaxLength, nameof(addressLine));
        Latitude = latitude;
        Longitude = longitude;
        GooglePlaceId = Trimmed(googlePlaceId, GooglePlaceIdMaxLength, nameof(googlePlaceId));
        GoogleMapsUrl = mapsUrl;
    }

    /// <summary>
    /// Replaces the campus's shifts with these, at most <see cref="MaxShiftsPerCampus"/>, with distinct names. The shifts it
    /// had are soft-deleted at <paramref name="at"/> by <paramref name="by"/>, so the old rows stay for audit. The campus must
    /// already be saved.
    /// </summary>
    public void ReplaceShifts(IReadOnlyList<(string Name, TimeOnly StartTime, TimeOnly EndTime)> shifts, DateTimeOffset at, string? by)
    {
        ArgumentNullException.ThrowIfNull(shifts);

        if (Id <= 0)
        {
            throw new InvalidOperationException("A campus is saved before it has shifts.");
        }

        if (shifts.Count > MaxShiftsPerCampus)
        {
            throw new ArgumentException($"A campus has at most {MaxShiftsPerCampus} shifts.", nameof(shifts));
        }

        var names = shifts.Select(s => s.Name.Trim()).ToList();
        if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
        {
            throw new ArgumentException("Shift names are unique within a campus.", nameof(shifts));
        }

        // Create checks each shift (name, length, end after start) before anything changes.
        var created = shifts.Select(s => CampusShift.Create(SchoolId, Id, s.Name, s.StartTime, s.EndTime)).ToList();

        foreach (var current in _shifts)
        {
            current.Delete(at, by);
        }

        _shifts.AddRange(created);
    }

    private static string? Trimmed(string? value, int maxLength, string paramName)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, maxLength, paramName);

        return trimmed;
    }
}
