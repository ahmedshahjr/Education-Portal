// Rebuilds src/Infrastructure/Data/Locations/locations.json from the Countries States Cities Database by dr5hn
// (https://github.com/dr5hn/countries-states-cities-database, ODbL v1.0). Run from "Education Portal Full Stack App":
//
//   dotnet run scripts/update-location-data.cs -- <commit-sha-or-tag>
//
// Pin a commit or tag, not a branch, so the file says exactly which data it holds. Review the diff, then open a PR.
// The app imports the file on start-up (LocationDataImporter); rows that administrators added (IsCustom) are never
// touched. The script fails if the data has a state type the app does not know, so add it to StateType first.
//
// What it keeps:
// - only the countries in CountryCodes;
// - states with an ISO 3166-2 code (this drops the three US military postal regions, which have none);
// - one city per name within a state (the dataset has a few towns twice under one name; the first one is kept).
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

string[] CountryCodes = ["SA", "AE", "QA", "KW", "BH", "OM", "US", "JP", "PK"];

// The dataset's state types, as the app's StateType enum names them.
var stateTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["state"] = "State",
    ["province"] = "Province",
    ["prefecture"] = "Prefecture",
    ["emirate"] = "Emirate",
    ["governorate"] = "Governorate",
    ["territory"] = "Territory",
    ["region"] = "Region",
    ["municipality"] = "Municipality",
    ["district"] = "District",
    ["administered area"] = "AdministeredArea",
    ["federal capital territory"] = "FederalCapitalTerritory",
    ["outlying area"] = "OutlyingArea",
};

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run scripts/update-location-data.cs -- <commit-sha-or-tag>");
    return 1;
}

var sourceRef = args[0].Trim();
var url = $"https://raw.githubusercontent.com/dr5hn/countries-states-cities-database/{Uri.EscapeDataString(sourceRef)}/json/countries%2Bstates%2Bcities.json";
var output = Path.Combine("src", "Infrastructure", "Data", "Locations", "locations.json");

if (!Directory.Exists(Path.GetDirectoryName(output)))
{
    Console.Error.WriteLine("Run this from the \"Education Portal Full Stack App\" folder.");
    return 1;
}

Console.WriteLine($"Downloading {url}");
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
await using var stream = await http.GetStreamAsync(url);
var dataset = (await JsonNode.ParseAsync(stream))!.AsArray();

var report = new StringBuilder();
var countries = new JsonArray();

foreach (var code in CountryCodes)
{
    var country = dataset.SingleOrDefault(c => (string?)c!["iso2"] == code)
        ?? throw new InvalidOperationException($"The dataset has no country {code}.");

    var states = new JsonArray();
    var cityCount = 0;

    foreach (var state in country["states"]!.AsArray().OrderBy(s => (string?)s!["iso3166_2"], StringComparer.Ordinal))
    {
        var name = (string)state!["name"]!;
        var isoCode = (string?)state["iso3166_2"];

        if (string.IsNullOrWhiteSpace(isoCode))
        {
            report.AppendLine($"{code}: skipped state without an ISO code: {name} ({state["type"]})");
            continue;
        }

        var type = (string?)state["type"] ?? string.Empty;
        if (!stateTypes.TryGetValue(type, out var stateType))
        {
            throw new InvalidOperationException($"{isoCode} {name} has the state type '{type}', which StateType does not have. Add it there and here.");
        }

        var cities = new JsonArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var city in state["cities"]!.AsArray())
        {
            var cityName = ((string)city!["name"]!).Trim();
            if (!seen.Add(cityName))
            {
                report.AppendLine($"{code}: skipped a second '{cityName}' in {name}");
                continue;
            }

            cities.Add((JsonNode)new JsonObject
            {
                ["name"] = cityName,
                ["latitude"] = Coordinate((string?)city["latitude"]),
                ["longitude"] = Coordinate((string?)city["longitude"]),
            });
        }

        var sorted = new JsonArray(cities.OrderBy(c => (string)c!["name"]!, StringComparer.Ordinal).Select(c => c!.DeepClone()).ToArray());
        cityCount += sorted.Count;

        states.Add((JsonNode)new JsonObject
        {
            ["isoCode"] = isoCode,
            ["name"] = name.Trim(),
            ["nativeName"] = Blank((string?)state["native"]),
            ["type"] = stateType,
            ["cities"] = sorted,
        });
    }

    countries.Add((JsonNode)new JsonObject
    {
        ["iso2"] = code,
        ["iso3"] = (string?)country["iso3"],
        ["name"] = (string?)country["name"],
        ["nativeName"] = Blank((string?)country["native"]),
        ["phoneCode"] = (string?)country["phonecode"],
        ["currency"] = (string?)country["currency"],
        ["states"] = states,
    });

    Console.WriteLine($"{code}: {states.Count} states, {cityCount} cities");
}

var document = new JsonObject
{
    ["source"] = new JsonObject
    {
        ["name"] = "Countries States Cities Database",
        ["url"] = "https://github.com/dr5hn/countries-states-cities-database",
        ["ref"] = sourceRef,
        ["license"] = "ODbL v1.0",
        ["retrieved"] = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
    },
    ["countries"] = countries,
};

// Indented, but one line per city, so the file stays small and a data update shows one diff line per changed city.
// Arabic, Urdu and Japanese stay readable instead of \u escapes.
var encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
var indented = new JsonSerializerOptions { WriteIndented = true, Encoder = encoder };
var compact = new JsonSerializerOptions { Encoder = encoder };

var cityPlaceholders = new List<string>();
foreach (var state in countries.SelectMany(c => c!["states"]!.AsArray()))
{
    var cities = state!["cities"]!.AsArray();
    for (var i = 0; i < cities.Count; i++)
    {
        cityPlaceholders.Add(cities[i]!.ToJsonString(compact));
        cities[i] = JsonValue.Create($"@@city-{cityPlaceholders.Count - 1}@@");
    }
}

var text = document.ToJsonString(indented);
text = System.Text.RegularExpressions.Regex.Replace(text, "\"@@city-(\\d+)@@\"", m => cityPlaceholders[int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)]);

await File.WriteAllTextAsync(output, text + "\n", new UTF8Encoding(false));
Console.WriteLine($"Wrote {output}");
Console.Write(report);
return 0;

static decimal? Coordinate(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
