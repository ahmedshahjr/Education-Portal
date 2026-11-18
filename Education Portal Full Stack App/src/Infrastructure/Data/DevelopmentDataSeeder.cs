using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Data;

/// <summary>
/// Development-only start-up data: the modules, the plans and price books, the master tenant and its administrator
/// account. Schools, campuses and people are not seeded any more; create them with the development data endpoint
/// (<c>POST /api/DevelopmentData/generate</c>) or through the API. It runs after the migrations and adds only what is
/// missing, so it is safe to run on every start.
/// </summary>
public class DevelopmentDataSeeder
{
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DevelopmentDataSeeder(ILogger<DevelopmentDataSeeder> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>The master tenant's domain in Development. The admin sign-in works on http(s)://localhost.</summary>
    public const string MasterDomain = "localhost";

    /// <summary>
    /// The password of every Development-only account, the seed administrator and the administrators of generated schools.
    /// It meets the 15-character minimum of SEC-AUTHN-03. Never used outside Development (SEC-SECRET-04).
    /// </summary>
    public const string DevelopmentPassword = "Local-Dev-Only-2026!";

    public async Task SeedAsync()
    {
        try
        {
            await SeedBaseDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the development data.");
            throw;
        }
    }

    private async Task SeedBaseDataAsync()
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

            foreach (var (moduleCode, requiresCode) in ModuleCatalog.Dependencies.Where(d => added.Contains(d.Module)))
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

        // The master tenant: exactly one, the platform owner.
        if (!await _context.Schools.AnyAsync(s => s.IsMaster))
        {
            _context.Schools.Add(School.CreateMaster("Education Portal", MasterDomain));
            await _context.SaveChangesAsync();
        }

        var master = await _context.Schools.SingleAsync(s => s.IsMaster);

        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        await CreateAdministratorAsync("administrator@localhost", master.Id, administratorRole.Name);
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
