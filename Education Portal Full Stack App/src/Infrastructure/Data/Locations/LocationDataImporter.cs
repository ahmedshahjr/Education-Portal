using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Data.Locations;

/// <summary>
/// Imports the countries, states and cities in <c>locations.json</c> (from the Countries States Cities Database,
/// ODbL v1.0) for the address dropdowns. It runs on every start-up and is safe to run again:
/// <list type="bullet">
/// <item>countries match on their ISO 3166-1 alpha-2 code, states on their ISO 3166-2 code (or their name in the
/// country), cities on their name in the state; a match is updated, never added twice;</item>
/// <item>rows an administrator added (<c>IsCustom</c>) are never changed;</item>
/// <item>a country the master tenant already has keeps its own name, languages, time zone and currency; only its ISO 3
/// code, phone code and native name come from the data;</item>
/// <item>rows that have left the dataset are kept, so addresses that use them stay valid.</item>
/// </list>
/// Rebuild the file with <c>scripts/update-location-data.cs</c>.
/// </summary>
public sealed class LocationDataImporter
{
    private const string ResourceName = "EducationPortal.Infrastructure.Data.Locations.locations.json";

    /// <summary>
    /// Official names that differ from the dataset's, by ISO 3166-2 code. Agreed with the maintainer on 2026-11-13; keep this
    /// list short and say why in the PR when adding to it.
    /// </summary>
    public static IReadOnlyDictionary<string, string> StateNameOverrides { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["PK-IS"] = "Islamabad Capital Territory",
        ["PK-JK"] = "Azad Jammu and Kashmir",
    };

    /// <summary>
    /// The regional settings a country gets when the import creates it (the dataset has no languages). The master tenant
    /// can change them afterwards. The portal speaks English, Arabic and Urdu only (Domain/Localization/Language.cs), so
    /// Japan starts with English until Japanese translations ship.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, CountryDefaults> NewCountryDefaults = new Dictionary<string, CountryDefaults>(StringComparer.Ordinal)
    {
        ["SA"] = new(["en", "ar"], "en", "Asia/Riyadh"),
        ["AE"] = new(["en", "ar"], "en", "Asia/Dubai"),
        // The United States has several time zones; the country's zone is only the fallback, each user's own zone wins.
        ["US"] = new(["en"], "en", "America/New_York"),
        ["JP"] = new(["en"], "en", "Asia/Tokyo"),
        ["PK"] = new(["en", "ur"], "en", "Asia/Karachi"),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // State types are written by name, like enums on the API (ENG-ENUM-02); an unknown name fails the import.
        Converters = { new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false) },
    };

    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocationDataImporter> _logger;

    public LocationDataImporter(ApplicationDbContext context, ILogger<LocationDataImporter> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Imports the data file built into the app.</summary>
    public async Task<IReadOnlyList<LocationImportResult>> ImportAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = typeof(LocationDataImporter).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The embedded resource {ResourceName} is missing.");

        var file = await JsonSerializer.DeserializeAsync<LocationFile>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The location data file is empty.");

        var results = new List<LocationImportResult>();

        foreach (var country in file.Countries)
        {
            results.Add(await ImportCountryAsync(country, cancellationToken));
        }

        return results;
    }

    private async Task<LocationImportResult> ImportCountryAsync(CountryRecord record, CancellationToken cancellationToken)
    {
        // There is no delete-country use case yet. When one is added, decide what the import does with a deleted
        // country: today it would try to add it again and stop on the unique Code index (fail loudly, never duplicate).
        var country = await _context.Countries.SingleOrDefaultAsync(c => c.Code == record.Iso2, cancellationToken);

        if (country is null)
        {
            if (!NewCountryDefaults.TryGetValue(record.Iso2, out var defaults))
            {
                throw new InvalidOperationException($"The location data has country {record.Iso2}, which has no regional defaults in {nameof(LocationDataImporter)}.");
            }

            country = Country.Create(record.Iso2, record.Name, defaults.Languages, defaults.DefaultLanguage, defaults.TimeZoneId, record.Currency);
            _context.Countries.Add(country);
        }

        country.SetReferenceCodes(record.Iso3, record.PhoneCode, record.NativeName);
        await _context.SaveChangesAsync(cancellationToken);

        var states = await _context.States.Where(s => s.CountryId == country.Id).ToListAsync(cancellationToken);
        var statesByIso = states.Where(s => s.IsoCode is not null).ToDictionary(s => s.IsoCode!, StringComparer.Ordinal);
        var statesByName = states.ToDictionary(s => s.Name, StringComparer.Ordinal);

        int statesAdded = 0, statesUpdated = 0, statesSkipped = 0;
        var cityTargets = new List<(State State, IReadOnlyList<CityRecord> Cities)>();

        foreach (var stateRecord in record.States)
        {
            var name = StateNameOverrides.GetValueOrDefault(stateRecord.IsoCode, stateRecord.Name);

            var state = statesByIso.GetValueOrDefault(stateRecord.IsoCode.ToUpperInvariant())
                ?? statesByName.GetValueOrDefault(name);

            if (state is null)
            {
                state = State.FromDataset(country.Id, stateRecord.IsoCode, name, stateRecord.NativeName, stateRecord.Type);
                _context.States.Add(state);
                statesByName[state.Name] = state;
                statesAdded++;
            }
            else if (state.IsCustom)
            {
                // An administrator added this state before the dataset had it; it stays as they made it, and the dataset's
                // cities are not imported under it.
                _logger.LogWarning("Location import left custom state {StateId} alone; the dataset now has {IsoCode} with the same name.", state.Id, stateRecord.IsoCode);
                statesSkipped++;
                continue;
            }
            else
            {
                state.UpdateFromDataset(name, stateRecord.NativeName, stateRecord.Type);
                if (_context.Entry(state).State == EntityState.Modified)
                {
                    statesUpdated++;
                }
            }

            cityTargets.Add((state, stateRecord.Cities));
        }

        await _context.SaveChangesAsync(cancellationToken);

        var stateIds = cityTargets.Select(t => t.State.Id).ToList();
        var cities = await _context.Cities.Where(c => stateIds.Contains(c.StateId)).ToListAsync(cancellationToken);
        var citiesByKey = cities.ToDictionary(c => (c.StateId, c.Name));

        int citiesAdded = 0, citiesUpdated = 0, citiesSkipped = 0;

        foreach (var (state, cityRecords) in cityTargets)
        {
            foreach (var cityRecord in cityRecords)
            {
                var key = (state.Id, cityRecord.Name.Trim());

                if (!citiesByKey.TryGetValue(key, out var city))
                {
                    city = City.FromDataset(state.Id, cityRecord.Name, cityRecord.Latitude, cityRecord.Longitude);
                    _context.Cities.Add(city);
                    citiesByKey[key] = city;
                    citiesAdded++;
                }
                else if (city.IsCustom)
                {
                    citiesSkipped++;
                }
                else
                {
                    city.UpdateFromDataset(cityRecord.Latitude, cityRecord.Longitude);
                    if (_context.Entry(city).State == EntityState.Modified)
                    {
                        citiesUpdated++;
                    }
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Detach what this country loaded, so the next country starts with an empty change tracker.
        _context.ChangeTracker.Clear();

        var result = new LocationImportResult(record.Iso2, statesAdded, statesUpdated, statesSkipped, citiesAdded, citiesUpdated, citiesSkipped);

        _logger.LogInformation(
            "Location import {CountryCode}: states {StatesAdded} added, {StatesUpdated} updated, {StatesSkipped} custom kept; cities {CitiesAdded} added, {CitiesUpdated} updated, {CitiesSkipped} custom kept.",
            result.CountryCode, result.StatesAdded, result.StatesUpdated, result.StatesSkipped, result.CitiesAdded, result.CitiesUpdated, result.CitiesSkipped);

        return result;
    }

    private sealed record CountryDefaults(string[] Languages, string DefaultLanguage, string TimeZoneId);

    private sealed record LocationFile(List<CountryRecord> Countries);

    private sealed record CountryRecord(string Iso2, string Iso3, string Name, string? NativeName, string? PhoneCode, string Currency, List<StateRecord> States);

    private sealed record StateRecord(string IsoCode, string Name, string? NativeName, StateType Type, List<CityRecord> Cities);

    private sealed record CityRecord(string Name, decimal? Latitude, decimal? Longitude);
}

/// <summary>What one import did for one country. Custom rows are counted as kept, never changed.</summary>
public sealed record LocationImportResult(
    string CountryCode,
    int StatesAdded,
    int StatesUpdated,
    int StatesSkipped,
    int CitiesAdded,
    int CitiesUpdated,
    int CitiesSkipped);
