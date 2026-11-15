using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A campus (the portal calls campuses campuses; the class keeps its name): where day-to-day data lives (AGENTS.md section 2).
/// Owned by the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). Every school has exactly one
/// <see cref="CampusKind.Main"/> campus, its registration campus, created by <see cref="School.Create"/> in the same save as
/// the school. A campus has a <see cref="Code"/>: four digits, generated when it is added, unique within its school. The
/// school may show it with a prefix (<see cref="School.CampusDisplayCode"/>).
/// </summary>
public class Campus : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 200;
    public const int CodeLength = 4;
    public const string MainName = "Main campus";

    /// <summary>The most phone numbers a campus can list.</summary>
    public const int MaxPhones = 5;

    /// <summary>The main campus is always the first code of a school.</summary>
    public const string MainCode = "0001";

    private Campus()
    {
        // For EF Core.
        Name = string.Empty;
        Code = string.Empty;
    }

    private Campus(int schoolId, string name, string code, CampusKind kind)
    {
        SchoolId = schoolId;
        Name = name;
        Code = code;
        Kind = kind;
    }

    /// <summary>Creates a campus that is not the main one, with the code the use case generated. The main campus comes only with its school.</summary>
    public static Campus Create(int schoolId, string name, string code)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        return new Campus(schoolId, name.Trim(), NormalizeCode(code), CampusKind.Other);
    }

    /// <summary>
    /// The school's registration campus. Its <see cref="SchoolId"/> is 0 until the school is saved; EF Core sets it
    /// through the school's campus collection, so the campus is saved with its school or not at all.
    /// </summary>
    internal static Campus CreateMain() => new(0, MainName, MainCode, CampusKind.Main);

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

    public CampusKind Kind { get; private set; }

    /// <summary>The street address. Optional.</summary>
    public string? Address { get; private set; }

    /// <summary>The state or province, from the location data. Optional.</summary>
    public int? StateId { get; private set; }

    public State? State { get; private set; }

    /// <summary>The city, from the location data. Set only together with a state.</summary>
    public int? CityId { get; private set; }

    public City? City { get; private set; }

    public string? PostalCode { get; private set; }

    private readonly List<CampusPhone> _phones = [];

    /// <summary>The campus's phone numbers, in the order the school gave them.</summary>
    public IReadOnlyList<CampusPhone> Phones => _phones;

    public string? Website { get; private set; }

    /// <summary>The school's registration number for the campus.</summary>
    public string? SchoolNumber { get; private set; }

    public string? ShortName { get; private set; }

    /// <summary>The campus's place on the map, in degrees. Set together with the details.</summary>
    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    /// <summary>The principal of the campus: a person of the same school whose user type is <see cref="UserType.Principal"/>.</summary>
    public int? PrincipalUserDetailsId { get; private set; }

    public UserDetails? PrincipalUserDetails { get; private set; }

    /// <summary>
    /// Replaces the campus's contact and registration details. The values were checked by <see cref="CampusDetails.Create"/>;
    /// the use case has checked that the state, city and principal belong to the campus's school and country.
    /// </summary>
    public void UpdateDetails(CampusDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted campus cannot be changed.");
        }

        Address = details.Address;
        StateId = details.StateId;
        CityId = details.CityId;
        PostalCode = details.PostalCode;
        ReplacePhones(details.Phones);
        Website = details.Website;
        SchoolNumber = details.SchoolNumber;
        ShortName = details.ShortName;
        Latitude = details.Latitude;
        Longitude = details.Longitude;
        PrincipalUserDetailsId = details.PrincipalUserDetailsId;
    }

    /// <summary>Replaces the phone numbers with the given ones, in order.</summary>
    private void ReplacePhones(IReadOnlyList<string> numbers)
    {
        _phones.Clear();
        for (var position = 0; position < numbers.Count; position++)
        {
            _phones.Add(new CampusPhone(this, numbers[position], position));
        }
    }

    /// <summary>The main campus is the school's registration campus: it cannot be deleted or deactivated.</summary>
    protected override bool CanBeDeactivated => Kind != CampusKind.Main;

    /// <summary>Changes the campus's name. The code is generated once and never changes.</summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
    }
}
