using Bogus;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.DevelopmentData.Common;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Data.DevelopmentData;

/// <summary>
/// Creates fake schools, campuses and people with Bogus, in batches, through the domain factories (so every rule still
/// applies). Each school is registered the same way the master tenant registers one (its country, default modules, trial
/// and first administrator), so it can be signed in to. The administrator's email is <c>admin@{domain}</c> and its password
/// is <see cref="DevelopmentDataSeeder.DevelopmentPassword"/>. Then the requested extra campuses, then its people.
/// The master tenant runs it, so the tenant filters are not used.
/// </summary>
public sealed class DevelopmentDataGenerator : IDevelopmentDataGenerator
{
    /// <summary>People saved per batch. Keeps memory flat for a million rows.</summary>
    public const int BatchSize = 2000;

    /// <summary>The first code given to an extra campus. Main campuses use their own code.</summary>
    private const int FirstCampusCode = 1001;

    private readonly ApplicationDbContext _context;
    private readonly IIdentityService _identity;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DevelopmentDataGenerator> _logger;

    public DevelopmentDataGenerator(
        ApplicationDbContext context,
        IIdentityService identity,
        TimeProvider timeProvider,
        ILogger<DevelopmentDataGenerator> logger)
    {
        _context = context;
        _identity = identity;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<DevelopmentDataSummary> GenerateAsync(DevelopmentDataRequest request, CancellationToken cancellationToken)
    {
        var country = await _context.Countries.SingleOrDefaultAsync(c => c.Code == request.CountryCode, cancellationToken)
            ?? throw new InvalidOperationException($"No country with code {request.CountryCode} exists. Import the location data first.");

        var faker = request.Seed is { } seed ? new Faker { Random = new Randomizer(seed) } : new Faker();

        var campuses = 0;
        var people = 0;

        for (var schoolNumber = 0; schoolNumber < request.Schools; schoolNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var domain = await FreeDomainAsync(faker, cancellationToken);
            var registered = await _context.ExecuteInTransactionAsync(token => SchoolRegistration.RegisterAsync(
                _context,
                _identity,
                _timeProvider,
                new SchoolRegistrationRequest(
                    faker.Company.CompanyName(),
                    domain,
                    country.PublicId,
                    DataRegion: null,
                    AdministratorEmail: $"admin@{domain}",
                    AdministratorPassword: DevelopmentDataSeeder.DevelopmentPassword),
                token), cancellationToken);
            var schoolId = registered.Id;

            for (var campusNumber = 0; campusNumber < request.CampusesPerSchool - 1; campusNumber++)
            {
                _context.Campuses.Add(Campus.Create(schoolId, faker.Address.City() + " Campus", (FirstCampusCode + campusNumber).ToString()));
                campuses++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var campusIds = await _context.Campuses.Where(c => c.SchoolId == schoolId).Select(c => c.Id).ToListAsync(cancellationToken);
            foreach (var _ in campusIds)
            {
                people += await AddPeopleAsync(faker, schoolId, request.PeoplePerCampus, cancellationToken);
            }

            // The school's people are saved; the tracked graph is dropped so memory does not grow with the run.
            _context.ChangeTracker.Clear();
        }

        _logger.LogInformation("Generated {Schools} schools, {Campuses} extra campuses and {People} people.", request.Schools, campuses, people);

        return new DevelopmentDataSummary(request.Schools, campuses + request.Schools, people);
    }

    /// <summary>Adds the people in batches and returns how many were added.</summary>
    private async Task<int> AddPeopleAsync(Faker faker, int schoolId, int count, CancellationToken cancellationToken)
    {
        // Students need an admission form, so the generator makes staff only; students are admitted through their own use case.
        UserType[] types = [UserType.Principal, UserType.Teacher, UserType.Staff];

        for (var added = 0; added < count; added++)
        {
            var middle = faker.Random.Bool(0.3f) ? faker.Name.FirstName() : null;
            _context.UserDetails.Add(UserDetails.Create(
                schoolId,
                faker.Name.FirstName(),
                middle,
                faker.Name.LastName(),
                faker.PickRandom(types),
                faker.Random.Replace("+92 3##-#######")));

            if ((added + 1) % BatchSize == 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _context.ChangeTracker.Clear();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return count;
    }

    /// <summary>A host name on <c>localhost</c> that no school has yet (domains are unique).</summary>
    private async Task<string> FreeDomainAsync(Faker faker, CancellationToken cancellationToken)
    {
        while (true)
        {
            var domain = $"{faker.Random.AlphaNumeric(8).ToLowerInvariant()}.localhost";

            if (!await _context.Schools.AnyAsync(s => s.Domain == domain, cancellationToken))
            {
                return domain;
            }
        }
    }
}
