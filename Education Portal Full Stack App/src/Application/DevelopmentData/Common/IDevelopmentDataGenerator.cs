namespace EducationPortal.Application.DevelopmentData.Common;

/// <summary>
/// Fills the database with fake schools, campuses and people for development and load testing. The implementation
/// lives in Infrastructure (it uses a fake-data library); the use case only validates the request and calls it.
/// </summary>
public interface IDevelopmentDataGenerator
{
    Task<DevelopmentDataSummary> GenerateAsync(DevelopmentDataRequest request, CancellationToken cancellationToken);
}

/// <param name="Schools">How many schools to create.</param>
/// <param name="CampusesPerSchool">Campuses per school, including its main campus.</param>
/// <param name="PeoplePerCampus">People created for each campus of each school.</param>
/// <param name="CountryCode">The ISO country code whose price book, languages and time zone the schools get (for example <c>PK</c>).</param>
/// <param name="Seed">A fixed seed makes the same data every run; <c>null</c> gives different data each time.</param>
public sealed record DevelopmentDataRequest(int Schools, int CampusesPerSchool, int PeoplePerCampus, string CountryCode, int? Seed);

/// <summary>What was created.</summary>
public sealed record DevelopmentDataSummary(int Schools, int Campuses, int People);
