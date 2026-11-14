namespace EducationPortal.Infrastructure.Data;

/// <summary>
/// Development-only sample schools: real Pakistani school names with the campuses they are known to run, for trying the
/// school and campus screens with realistic data. Each school is reached on <c>{Slug}.localhost</c>, so the sample never
/// claims a real school's website. The campus lists come from public sources and are not complete or current: the
/// master tenant can edit them, and none of this data is used outside Development (SEC-CFG-03).
/// </summary>
public static class RealSchoolCatalog
{
    /// <summary>A school and its campuses, besides its registration campus (the main campus is added with the school).</summary>
    public sealed record SampleSchool(string Name, string Slug, IReadOnlyList<string> Campuses);

    public static IReadOnlyList<SampleSchool> Schools { get; } =
    [
        new("Beaconhouse School System", "beaconhouse", ["Lahore", "Karachi", "Islamabad", "Peshawar", "Multan"]),
        new("The City School", "city-school", ["Karachi Gulshan", "Lahore", "Islamabad", "Faisalabad", "Sialkot"]),
        new("Lahore Grammar School", "lahore-grammar", ["Lahore 55 Main Gulberg", "Lahore 30 Main Gulberg", "Lahore 31 FCC Gulberg", "Islamabad", "Karachi"]),
        new("Karachi Grammar School", "karachi-grammar", ["Saddar", "Clifton", "O and A Levels Campus"]),
        new("Allied Schools", "allied-schools", ["Mirpur", "Rahim Yar Khan", "Faisalabad", "Gujranwala"]),
        new("The Educators", "the-educators", ["Okara Al-Aleem", "Multan Shah Rukn-e-Alam", "Multan Mumtazabad", "Sahiwal", "Dera Ghazi Khan"]),
        new("Roots International Schools", "roots-international", ["Islamabad Columbia Park", "Karachi Kingswood", "Lahore Askari", "Peshawar Zamrud", "Chakwal Phoenix"]),
        new("Divisional Public School", "divisional-public", ["Lahore Model Town", "Faisalabad", "Sahiwal", "Sargodha", "Rawalpindi", "Okara City"]),
        new("Pakistan International Public School and College", "pakistan-international", ["Abbottabad", "Mansehra", "Swabi", "Mirpur", "Gujranwala", "Gujrat"]),
        new("Foundation Public School", "foundation-public", ["Karachi", "Hyderabad"]),
    ];
}
