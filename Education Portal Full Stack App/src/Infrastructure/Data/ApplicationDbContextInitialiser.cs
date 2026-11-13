using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data.Locations;
using EducationPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        // EnsureDeleted drops the database, so this must never run outside Development (SEC-CFG-03).
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("The database is only reset and seeded in the Development environment.");
        }

        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        // The location data comes before the demo seed, which assigns its schools to the imported countries.
        await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();

        await initialiser.SeedAsync();
    }

    /// <summary>
    /// Imports the built-in countries, states and cities (<see cref="LocationDataImporter"/>). Runs on every start-up outside
    /// Development, where <see cref="InitialiseDatabaseAsync"/> does it; safe to run again. The schema must already exist.
    /// </summary>
    public static async Task ImportLocationDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    /// <summary>The master tenant's domain in Development. The admin sign-in works on http(s)://localhost.</summary>
    public const string MasterDomain = "localhost";

    /// <summary>
    /// A demo school for trying per-school branding. Browsers resolve every *.localhost name to this machine, so it
    /// opens on demo.localhost with the same port.
    /// </summary>
    public const string DemoSchoolDomain = "demo.localhost";

    /// <summary>A second demo school, in the United Arab Emirates, for trying the per-country languages (English and Arabic).</summary>
    public const string UaeDemoSchoolDomain = "uae.localhost";

    /// <summary>A third demo school, in the United States, for the US market (English, en-US formats, US dollars).</summary>
    public const string UsaDemoSchoolDomain = "usa.localhost";

    // Development-only seed accounts (SEC-SECRET-04). It meets the 15-character minimum of SEC-AUTHN-03.
    private const string DevelopmentPassword = "Local-Dev-Only-2026!";

    public async Task TrySeedAsync()
    {
        // Modules: add any the code knows and the database lacks. The master tenant decides afterwards which are default.
        var knownModules = await _context.Modules.Select(m => m.Key).ToListAsync();
        var missingModules = ModuleCatalog.CreateModules().Where(m => !knownModules.Contains(m.Key)).ToList();

        if (missingModules.Count > 0)
        {
            _context.Modules.AddRange(missingModules);
            await _context.SaveChangesAsync();
        }

        // Countries come from the location data import, which runs before this seed (LocationDataImporter).
        var pakistan = await _context.Countries.SingleAsync(c => c.Code == "PK");
        var uae = await _context.Countries.SingleAsync(c => c.Code == "AE");
        var usa = await _context.Countries.SingleAsync(c => c.Code == "US");

        // Tenants: exactly one master tenant (the platform owner) and one demo school per market.
        var master = await _context.Schools.SingleOrDefaultAsync(s => s.IsMaster);
        if (master is null)
        {
            master = School.CreateMaster("Education Portal", MasterDomain);
            _context.Schools.Add(master);
        }

        if (!await _context.Schools.AnyAsync(s => s.Domain == DemoSchoolDomain))
        {
            var demo = School.Create("Demo School", DemoSchoolDomain);
            demo.AssignCountry(pakistan.Id);
            _context.Schools.Add(demo);
        }

        if (!await _context.Schools.AnyAsync(s => s.Domain == UaeDemoSchoolDomain))
        {
            var uaeDemo = School.Create("Demo School UAE", UaeDemoSchoolDomain);
            uaeDemo.AssignCountry(uae.Id);
            _context.Schools.Add(uaeDemo);
        }

        if (!await _context.Schools.AnyAsync(s => s.Domain == UsaDemoSchoolDomain))
        {
            var usaDemo = School.Create("Demo School USA", UsaDemoSchoolDomain);
            usaDemo.AssignCountry(usa.Id);
            _context.Schools.Add(usaDemo);
        }

        await _context.SaveChangesAsync();

        var demoSchool = await _context.Schools.SingleAsync(s => s.Domain == DemoSchoolDomain);
        var uaeDemoSchool = await _context.Schools.SingleAsync(s => s.Domain == UaeDemoSchoolDomain);
        var usaDemoSchool = await _context.Schools.SingleAsync(s => s.Domain == UsaDemoSchoolDomain);

        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost", SchoolId = master.Id };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, DevelopmentPassword);
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // The demo school's administrator signs in on demo.localhost only.
        var demoAdministrator = new ApplicationUser { UserName = "administrator@demo.localhost", Email = "administrator@demo.localhost", SchoolId = demoSchool.Id };

        if (_userManager.Users.All(u => u.UserName != demoAdministrator.UserName))
        {
            await _userManager.CreateAsync(demoAdministrator, DevelopmentPassword);
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(demoAdministrator, new [] { administratorRole.Name });
            }
        }

        // The UAE demo school's administrator signs in on uae.localhost only.
        var uaeAdministrator = new ApplicationUser { UserName = "administrator@uae.localhost", Email = "administrator@uae.localhost", SchoolId = uaeDemoSchool.Id };

        if (_userManager.Users.All(u => u.UserName != uaeAdministrator.UserName))
        {
            await _userManager.CreateAsync(uaeAdministrator, DevelopmentPassword);
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(uaeAdministrator, new [] { administratorRole.Name });
            }
        }

        // The USA demo school's administrator signs in on usa.localhost only.
        var usaAdministrator = new ApplicationUser { UserName = "administrator@usa.localhost", Email = "administrator@usa.localhost", SchoolId = usaDemoSchool.Id };

        if (_userManager.Users.All(u => u.UserName != usaAdministrator.UserName))
        {
            await _userManager.CreateAsync(usaAdministrator, DevelopmentPassword);
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(usaAdministrator, new [] { administratorRole.Name });
            }
        }
    }
}
