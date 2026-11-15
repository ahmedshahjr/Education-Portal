using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Data;

/// <summary>
/// Development-only demo data: the master tenant, demo schools, their subscriptions, campuses, invoices, roles and
/// sign-in accounts. It runs after the migrations and adds only what is missing, so it is safe to run on every start.
/// </summary>
public class DevelopmentDataSeeder
{
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TimeProvider _timeProvider;

    public DevelopmentDataSeeder(ILogger<DevelopmentDataSeeder> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
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

    public async Task SeedAsync()
    {
        try
        {
            await SeedDemoDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the development data.");
            throw;
        }
    }

    private async Task SeedDemoDataAsync()
    {
        // Modules and their dependencies: add any the code knows and the database lacks. The master tenant changes them
        // afterwards.
        var knownModules = await _context.Modules.Select(m => m.Code).ToListAsync();
        var missingModules = ModuleCatalog.CreateModules().Where(m => !knownModules.Contains(m.Code)).ToList();

        if (missingModules.Count > 0)
        {
            _context.Modules.AddRange(missingModules);
            await _context.SaveChangesAsync();

            var modules = await _context.Modules.ToDictionaryAsync(m => m.Code);
            var added = missingModules.Select(m => m.Code).ToHashSet();

            foreach (var (moduleCode, requiresCode) in ModuleCatalog.Dependencies.Where(d => added.Contains(d.ModuleCode)))
            {
                if (modules.TryGetValue(moduleCode, out var module) && modules.TryGetValue(requiresCode, out var requires))
                {
                    _context.ModuleDependencies.Add(ModuleDependency.Create(module, requires));
                }
            }

            await _context.SaveChangesAsync();
        }

        // Plans and price books: placeholder prices for the master tenant to replace.
        await SeedPricingAsync();

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

        // The demo schools registered like any new school: they get the modules that are on by default.
        foreach (var school in new[] { demoSchool, uaeDemoSchool, usaDemoSchool })
        {
            await SchoolDefaultModules.GrantAsync(_context, school.Id, _timeProvider.GetUtcNow(), CancellationToken.None);
        }

        await _context.SaveChangesAsync();

        // The demo schools run on the Full plan for a year, so every module works when developing.
        await SeedDemoSubscriptionAsync(demoSchool, pakistan);
        await SeedDemoSubscriptionAsync(uaeDemoSchool, uae);
        await SeedDemoSubscriptionAsync(usaDemoSchool, usa);

        // The Pakistan and UAE demo schools have campuses besides their main campus, so the campus screens have data.
        await SeedDemoCampusesAsync(demoSchool, KarachiHeadOffice, [KarachiCampus, LahoreCampus]);
        await SeedDemoCampusesAsync(uaeDemoSchool, DubaiHeadOffice, [DubaiCampus, AbuDhabiCampus]);

        // Each demo school has an open invoice for its first period and two school roles, so the billing and roles screens have data.
        await SeedDemoInvoiceAsync(demoSchool);
        await SeedDemoInvoiceAsync(uaeDemoSchool);
        await SeedDemoRolesAsync(demoSchool);
        await SeedDemoRolesAsync(uaeDemoSchool);

        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        await CreateAdministratorAsync("administrator@localhost", master.Id, administratorRole.Name);

        // Each demo school's administrator signs in on that school's domain only.
        await CreateAdministratorAsync("administrator@demo.localhost", demoSchool.Id, administratorRole.Name);
        await CreateAdministratorAsync("administrator@uae.localhost", uaeDemoSchool.Id, administratorRole.Name);
        await CreateAdministratorAsync("administrator@usa.localhost", usaDemoSchool.Id, administratorRole.Name);
    }

    private async Task CreateAdministratorAsync(string userName, int schoolId, string? roleName)
    {
        if (await _userManager.Users.AnyAsync(u => u.UserName == userName))
        {
            return;
        }

        var administrator = new ApplicationUser { UserName = userName, Email = userName, SchoolId = schoolId };
        await _userManager.CreateAsync(administrator, DevelopmentPassword);

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            await _userManager.AddToRolesAsync(administrator, new[] { roleName });
        }
    }

    /// <summary>Issues an open invoice for a demo school's first period. Safe to run again: a subscription with an invoice is skipped.</summary>
    private async Task SeedDemoInvoiceAsync(School school)
    {
        // The seed runs with no tenant, so the tenant filters would hide the rows: ignore them, scoped to this school.
        var subscription = await _context.Subscriptions.IgnoreQueryFilters().SingleAsync(s => s.SchoolId == school.Id);
        if (await _context.Invoices.IgnoreQueryFilters().AnyAsync(i => i.SubscriptionId == subscription.Id))
        {
            return;
        }

        var book = await _context.PriceBooks.SingleAsync(p => p.Id == subscription.PriceBookId);
        _context.Invoices.Add(Invoice.Create(subscription, book.TaxRate, _timeProvider.GetUtcNow(), nextPeriod: false, "Full plan, first period (demo)"));
        await _context.SaveChangesAsync();
    }

    /// <summary>Adds two school roles with rights in the demo school's modules. Safe to run again: a role whose name exists is skipped.</summary>
    private async Task SeedDemoRolesAsync(School school)
    {
        var modules = await _context.Modules.ToDictionaryAsync(m => m.Code);
        var roles = new (string Name, (string Code, bool View, bool Create, bool Edit, bool Delete)[] Rights)[]
        {
            ("Registrar", [(ModuleCodes.Students, true, true, true, false), (ModuleCodes.AcademicSetup, true, false, false, false)]),
            ("Accountant", [(ModuleCodes.Fees, true, true, true, false)]),
        };

        foreach (var (name, rights) in roles)
        {
            if (await _context.SchoolRoles.IgnoreQueryFilters().AnyAsync(r => r.SchoolId == school.Id && r.Name == name))
            {
                continue;
            }

            var role = SchoolRole.Create(school.Id, name);
            _context.SchoolRoles.Add(role);
            await _context.SaveChangesAsync();

            foreach (var (code, view, create, edit, delete) in rights)
            {
                _context.RolePermissions.Add(RolePermission.Create(role, modules[code], view, create, edit, delete));
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// A demo campus with its full details: the province and city come from the location data of the school's country, and
    /// the place on the map is its coordinates. The main campus takes the same details as the school's head office.
    /// </summary>
    private sealed record DemoCampus(
        string Name,
        string Code,
        string StateName,
        string CityName,
        string Address,
        string PostalCode,
        string[] Phones,
        string SchoolNumber,
        string ShortName,
        decimal Latitude,
        decimal Longitude);

    private static readonly DemoCampus KarachiHeadOffice = new(
        "Karachi", "0001", "Sindh", "Karachi", "Shahrah-e-Faisal, Karachi", "74200", ["+92 21 34567890"], "PK-KHI-001", "KHI", 24.8607m, 67.0011m);

    private static readonly DemoCampus KarachiCampus = new(
        "Karachi Clifton", "0002", "Sindh", "Karachi", "Clifton Block 4, Karachi", "75600", ["+92 21 35870001"], "PK-KHI-002", "KHI", 24.8138m, 67.0300m);

    private static readonly DemoCampus LahoreCampus = new(
        "Lahore", "0003", "Punjab", "Lahore", "Main Boulevard, Gulberg III, Lahore", "54660", ["+92 42 35761234", "+92 42 35761235"], "PK-LHR-001", "LHR", 31.5204m, 74.3587m);

    private static readonly DemoCampus DubaiHeadOffice = new(
        "Dubai", "0001", "Dubai", "Dubai", "Sheikh Zayed Road, Dubai", "00000", ["+971 4 3001234"], "AE-DXB-001", "DXB", 25.2048m, 55.2708m);

    private static readonly DemoCampus DubaiCampus = new(
        "Dubai Al Barsha", "0002", "Dubai", "Al Barsha", "Al Barsha 1, Dubai", "00000", ["+971 4 3005678"], "AE-DXB-002", "DXB2", 25.1131m, 55.2000m);

    private static readonly DemoCampus AbuDhabiCampus = new(
        "Abu Dhabi", "0003", "Abu Dhabi", "Abu Dhabi", "Khalifa Street, Abu Dhabi", "00000", ["+971 2 6001234"], "AE-AUH-001", "AUH", 24.4539m, 54.3773m);

    /// <summary>
    /// Gives a demo school its main campus (the head office, which keeps its name) and the extra campuses, with their full
    /// details, so the campus screens show real data. Safe to run again: a campus whose code exists keeps its details, and one
    /// with no details yet gets them.
    /// </summary>
    private async Task SeedDemoCampusesAsync(School school, DemoCampus headOffice, DemoCampus[] campuses)
    {
        // The seed runs with no tenant, so the campus query filter would hide existing rows: ignore it, scoped to this school.
        var main = await _context.Campuses.IgnoreQueryFilters()
            .SingleAsync(b => b.SchoolId == school.Id && b.Kind == CampusKind.Main);
        await ApplyDemoDetailsAsync(school, main, headOffice);

        foreach (var demo in campuses)
        {
            var campus = await _context.Campuses.IgnoreQueryFilters()
                .SingleOrDefaultAsync(b => b.SchoolId == school.Id && b.Code == demo.Code);
            if (campus is null)
            {
                campus = Campus.Create(school.Id, demo.Name, demo.Code);
                _context.Campuses.Add(campus);
                await _context.SaveChangesAsync();
            }

            await ApplyDemoDetailsAsync(school, campus, demo);
        }
    }

    /// <summary>Fills a campus's details when it has none yet. A campus that already has details is left as it is.</summary>
    private async Task ApplyDemoDetailsAsync(School school, Campus campus, DemoCampus demo)
    {
        if (campus.Address is not null || school.CountryId is not { } countryId)
        {
            return;
        }

        var state = await _context.States.SingleAsync(s => s.CountryId == countryId && s.Name == demo.StateName);
        var city = await _context.Cities.SingleAsync(c => c.StateId == state.Id && c.Name == demo.CityName);

        var details = CampusDetails.Create(
            demo.Address,
            state.Id,
            city.Id,
            demo.PostalCode,
            demo.Phones,
            website: null,
            demo.SchoolNumber,
            demo.ShortName,
            principalUserDetailsId: null,
            demo.Latitude,
            demo.Longitude);
        campus.UpdateDetails(details);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gives a demo school a one-year subscription to the Full plan, priced from its country's price book. Safe to run
    /// again: a school that already has a subscription is skipped, because a second one would break the one-per-school rule.
    /// </summary>
    private async Task SeedDemoSubscriptionAsync(School school, Country country)
    {
        // The seed runs with no tenant, so the tenant filters would hide the rows: ignore them, scoped to this school.
        if (await _context.Subscriptions.IgnoreQueryFilters().AnyAsync(s => s.SchoolId == school.Id))
        {
            return;
        }

        var plan = await _context.Plans.SingleAsync(p => p.Code == PricingCatalog.Full);
        var book = await _context.PriceBooks.SingleAsync(b => b.CountryCode == country.Code);
        var price = await _context.PriceBookItems.SingleAsync(i => i.PriceBookId == book.Id && i.PlanId == plan.Id && i.BillingCycle == BillingCycle.Yearly);
        var planModuleIds = await _context.PlanModules.Where(m => m.PlanId == plan.Id).Select(m => m.ModuleId).ToListAsync();

        var subscription = Subscription.Create(school.Id, plan, book, BillingCycle.Yearly, price.Price, _timeProvider.GetUtcNow(), trialDays: 0);
        // The seed runs with no tenant, so the campus query filter would hide the campus: ignore it, scoped to this school.
        var mainCampus = await _context.Campuses.IgnoreQueryFilters()
            .SingleAsync(b => b.SchoolId == school.Id && b.Kind == CampusKind.Main);
        foreach (var module in await _context.Modules.Where(m => m.IsCore || planModuleIds.Contains(m.Id)).ToListAsync())
        {
            subscription.AddModule(module, mainCampus);
        }

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    /// <summary>Adds the starting plans, and the price books of the countries that exist. Safe to run again: it adds only what is missing.</summary>
    public async Task SeedPricingAsync()
    {
        var modules = await _context.Modules.ToDictionaryAsync(m => m.Code);
        var knownPlans = await _context.Plans.Select(p => p.Code).ToListAsync();

        foreach (var (code, name, description, moduleCodes) in PricingCatalog.Plans.Where(p => !knownPlans.Contains(p.Code)))
        {
            var plan = Plan.Create(code, name, description);
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            _context.PlanModules.AddRange(moduleCodes.Where(modules.ContainsKey).Select(m => PlanModule.Create(plan, modules[m])));
            await _context.SaveChangesAsync();
        }

        var plans = await _context.Plans.ToDictionaryAsync(p => p.Code);
        var knownBooks = await _context.PriceBooks.Select(b => b.CountryCode).ToListAsync();
        var countries = await _context.Countries.Select(c => c.Code).ToListAsync();
        var paidModules = modules.Values.Where(m => !m.IsCore).ToList();

        foreach (var (countryCode, currencyCode, planPrices, modulePrice) in PricingCatalog.PriceBooks
                     .Where(b => !knownBooks.Contains(b.CountryCode) && countries.Contains(b.CountryCode)))
        {
            var book = PriceBook.Create(countryCode, currencyCode);
            _context.PriceBooks.Add(book);
            await _context.SaveChangesAsync();

            foreach (var (planCode, monthly) in planPrices.Where(p => plans.ContainsKey(p.Key)))
            {
                _context.PriceBookItems.Add(PriceBookItem.ForPlan(book, plans[planCode], BillingCycle.Monthly, monthly));
                _context.PriceBookItems.Add(PriceBookItem.ForPlan(book, plans[planCode], BillingCycle.Yearly, monthly * PricingCatalog.MonthsChargedPerYear));
            }

            foreach (var module in paidModules)
            {
                _context.PriceBookItems.Add(PriceBookItem.ForModule(book, module, BillingCycle.Monthly, modulePrice));
                _context.PriceBookItems.Add(PriceBookItem.ForModule(book, module, BillingCycle.Yearly, modulePrice * PricingCatalog.MonthsChargedPerYear));
            }

            await _context.SaveChangesAsync();
        }
    }
}
