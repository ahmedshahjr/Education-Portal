using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Infrastructure.Identity;
using EducationPortal.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

public static class TestApp
{
    public const string DefaultUserEmail = "default.user@local";
    public const string AdministratorEmail = "administrator@local";

    /// <summary>The master tenant's domain. Test clients call https://localhost, so they reach the master tenant.</summary>
    public const string MasterDomain = "localhost";

    // The passwords are random for each test run. They exist only in the disposable test database, so no credential is
    // written into the code (SEC-AI-03, SEC-SECRET-04).
    public static string DefaultUserPassword { get; } = NewPassword();

    public static string AdministratorPassword { get; } = NewPassword();

    private static string? _userId;
    private static List<string>? _roles;
    private static ResolvedTenant? _tenant;

    /// <summary>The master tenant, created again after each database reset.</summary>
    /// <summary>The master tenant's internal key, for the tenant and users in tests.</summary>
    public static int MasterSchoolId { get; private set; }

    /// <summary>The master tenant's public id, for API routes (ENG-ID-02).</summary>
    public static Guid MasterSchoolPublicId { get; private set; }

    /// <summary>Sends a request as the current user, on the tenant chosen by <see cref="UseTenant"/> (the master by default).</summary>
    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = CreateTenantScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = CreateTenantScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    /// <summary>Makes <see cref="SendAsync{TResponse}"/> run as if the request came to this school's domain.</summary>
    public static void UseTenant(int schoolId, bool isMaster = false) => _tenant = new ResolvedTenant(schoolId, isMaster);

    /// <summary>Adds a school with its own domain and returns its id.</summary>
    public static async Task<EntityIds> AddSchoolAsync(string name, string domain)
    {
        var school = School.Create(name, domain);
        await AddAsync(school);
        return new EntityIds(school.Id, school.PublicId);
    }

    /// <summary>Adds a country with its regional settings and returns its id.</summary>
    public static async Task<EntityIds> AddCountryAsync(string code, string name, string[] languages, string defaultLanguage, string timeZoneId, string currencyCode)
    {
        var country = Country.Create(code, name, languages, defaultLanguage, timeZoneId, currencyCode);
        await AddAsync(country);
        return new EntityIds(country.Id, country.PublicId);
    }

    /// <summary>Adds a school with its own domain in a country and returns its id.</summary>
    public static async Task<EntityIds> AddSchoolAsync(string name, string domain, int countryId)
    {
        var school = School.Create(name, domain);
        school.AssignCountry(countryId);
        await AddAsync(school);
        return new EntityIds(school.Id, school.PublicId);
    }

    /// <summary>The ids of the school on a domain, or <c>null</c>.</summary>
    public static async Task<EntityIds?> FindSchoolIdAsync(string domain)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Schools.Where(s => s.Domain == domain).Select(s => new EntityIds(s.Id, s.PublicId)).FirstOrDefaultAsync();
    }

    /// <summary>The internal key of the row with a public id (ENG-ID-03). Test helpers run as the master tenant, so the tenant filters do not hide it.</summary>
    public static async Task<int> FindIdAsync<TEntity>(Guid publicId)
        where TEntity : EducationPortal.Domain.Common.BaseEntity
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().Where(e => e.PublicId == publicId).Select(e => e.Id).SingleAsync();
    }

    public static string? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

    /// <summary>Runs with no signed-in user, as a scheduled job does.</summary>
    public static void RunAsSystem()
    {
        _userId = null;
        _roles = null;
    }

    /// <summary>Creates a user of the master tenant without roles and runs as that user.</summary>
    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync(DefaultUserEmail, DefaultUserPassword, []);
    }

    /// <summary>Creates an administrator of the master tenant and runs as that user.</summary>
    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync(AdministratorEmail, AdministratorPassword, [Roles.Administrator]);
    }

    public static Task<string> RunAsUserAsync(string userName, string password, string[] roles) =>
        RunAsUserAsync(MasterSchoolId, userName, password, roles);

    /// <summary>Creates a user of <paramref name="schoolId"/> and runs as that user, on that school's tenant.</summary>
    public static async Task<string> RunAsUserAsync(int schoolId, string userName, string password, string[] roles)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = userName, Email = userName, SchoolId = schoolId };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _userId = user.Id;
            _roles = [..roles];
            UseTenant(schoolId, schoolId == MasterSchoolId);
            return _userId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    /// <summary>
    /// Gives a school a subscription that grants the paid modules, without going through plans and pricing (those have
    /// their own tests). The subscription starts at <paramref name="start"/> (now by default) and runs one month, or is a
    /// trial of <paramref name="trialDays"/> days, at <paramref name="amount"/> a month; with a branch, the modules are for that branch only. The school's cached
    /// entitlements are dropped, as after a real change.
    /// </summary>
    public static async Task<Subscription> SubscribeAsync(int schoolId, string[] moduleCodes, DateTimeOffset? start = null, int? branchId = null, int trialDays = 0, decimal amount = 0m)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var book = await context.PriceBooks.FirstOrDefaultAsync(b => b.CountryCode == "XX");
        if (book is null)
        {
            book = PriceBook.Create("XX", "XXX");
            context.PriceBooks.Add(book);
            await context.SaveChangesAsync();
        }

        // Without a branch the modules are for the whole school: they go on its main branch.
        var branch = branchId is null
            ? await context.Branches.SingleAsync(b => b.SchoolId == schoolId && b.Kind == EducationPortal.Domain.Enums.BranchKind.Main)
            : await context.Branches.FirstAsync(b => b.Id == branchId);
        var subscription = Subscription.Create(schoolId, null, book, BillingCycle.Monthly, amount, start ?? DateTimeOffset.UtcNow, trialDays);
        foreach (var module in await context.Modules.Where(m => moduleCodes.Contains(m.Code)).ToListAsync())
        {
            subscription.AddModule(module, branch);
        }

        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        scope.ServiceProvider.GetRequiredService<IModuleEntitlementStore>().Invalidate(schoolId);

        return subscription;
    }

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;

        // Module entitlements are cached per school; the database was just emptied.
        FunctionalTestSetup.ScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IModuleEntitlementStore>().InvalidateAll();

        // Every request needs a tenant, so the master tenant is created again after each reset.
        var master = School.CreateMaster("Education Portal", MasterDomain);
        await AddAsync(master);

        // The module catalogue and its dependencies are seeded data, so they are added again too.
        var modules = ModuleCatalog.CreateModules().ToDictionary(m => m.Code);
        foreach (var module in modules.Values)
        {
            await AddAsync(module);
        }

        foreach (var (moduleCode, requiresCode) in ModuleCatalog.Dependencies)
        {
            await AddAsync(ModuleDependency.Create(modules[moduleCode], modules[requiresCode]));
        }
        MasterSchoolId = master.Id;
        MasterSchoolPublicId = master.PublicId;
        UseTenant(MasterSchoolId, isMaster: true);
    }

    /// <summary>The audit records written for one user, oldest first.</summary>
    public static async Task<List<SecurityAuditEvent>> FindAuditEventsAsync(string actorUserId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.SecurityAuditEvents
            .Where(e => e.ActorUserId == actorUserId)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    /// <summary>Counts rows as the current tenant sees them (the master by default), so tenant query filters apply.</summary>
    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = CreateTenantScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    private static IServiceScope CreateTenantScope()
    {
        var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        if (_tenant is not null)
        {
            scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(_tenant);
        }

        return scope;
    }

    private static string NewPassword() => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
}

/// <summary>
/// The two identities of a row a test added (ENG-ID-01, ENG-ID-02): <see cref="Id"/> for the tenant and users set up in
/// the test, <see cref="PublicId"/> for API routes and bodies, which never take the internal key.
/// </summary>
public sealed record EntityIds(int Id, Guid PublicId);
