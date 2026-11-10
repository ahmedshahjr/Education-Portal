using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
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
        await initialiser.SeedAsync();
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

    // Development-only seed accounts (SEC-SECRET-04). It meets the 15-character minimum of SEC-AUTHN-03.
    private const string DevelopmentPassword = "Local-Dev-Only-2026!";

    public async Task TrySeedAsync()
    {
        // Countries: the master tenant manages these; two examples so each demo school offers its own languages.
        if (!await _context.Countries.AnyAsync())
        {
            _context.Countries.AddRange(
                Country.Create("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR"),
                Country.Create("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED"));

            await _context.SaveChangesAsync();
        }

        var pakistan = await _context.Countries.SingleAsync(c => c.Code == "PK");
        var uae = await _context.Countries.SingleAsync(c => c.Code == "AE");

        // Tenants: exactly one master tenant (the platform owner) and one demo school.
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

        await _context.SaveChangesAsync();

        var demoSchool = await _context.Schools.SingleAsync(s => s.Domain == DemoSchoolDomain);
        var uaeDemoSchool = await _context.Schools.SingleAsync(s => s.Domain == UaeDemoSchoolDomain);

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
    }
}
