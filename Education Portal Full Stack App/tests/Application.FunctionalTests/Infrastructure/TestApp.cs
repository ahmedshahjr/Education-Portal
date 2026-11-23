using System.Reflection;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Infrastructure.Identity;
using EducationPortal.Web.Services;
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

    /// <summary>
    /// Runs a use case as the current user, on the tenant chosen by <see cref="UseTenant"/> (the master by default). It applies
    /// the same two rules the controllers' filters apply (ADR-025), the authorisation and the validation, and then calls the
    /// use case's Handle method, so a denial test sees the same error the API returns.
    /// </summary>
    public static async Task<TResponse> SendAsync<TResponse>(object request)
    {
        var result = await InvokeAsync(request);
        return (TResponse)result!;
    }

    /// <summary>Runs a use case that returns nothing (see <see cref="SendAsync{TResponse}"/>).</summary>
    public static async Task SendAsync(object request)
    {
        await InvokeAsync(request);
    }

    private static async Task<object?> InvokeAsync(object request)
    {
        using var scope = CreateTenantScope();
        var services = scope.ServiceProvider;
        var requestType = request.GetType();

        // The same rules a controller action applies to its request, before the use case runs.
        await services.GetRequiredService<IRequestRules>().CheckAsync(request, CancellationToken.None);

        var handlerType = requestType.Assembly.GetType(requestType.FullName + "Handler")
            ?? throw new InvalidOperationException($"No use case service for {requestType.Name}.");
        var handler = services.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("Handle", [requestType, typeof(CancellationToken)])
            ?? throw new InvalidOperationException($"{handlerType.Name} has no Handle method.");

        try
        {
            var task = (Task)method.Invoke(handler, [request, CancellationToken.None])!;
            await task;

            // A use case that returns a value has a Task<T>; the result is read from it.
            return task.GetType().IsGenericType ? task.GetType().GetProperty("Result")!.GetValue(task) : null;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Makes <see cref="SendAsync{TResponse}"/> run as if the request came to this school's domain.</summary>
    public static void UseTenant(int schoolId, bool isMaster = false) => _tenant = Resolve(schoolId, isMaster);

    /// <summary>The tenant a school belongs to, resolved the way the host resolves it (school and tenant ids, the master flag).</summary>
    public static ResolvedTenant Resolve(int schoolId, bool isMaster = false)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var school = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Schools.Single(s => s.Id == schoolId);

        return new ResolvedTenant(schoolId, school.TenantId, isMaster);
    }

    /// <summary>
    /// Sets the tenant of a test scope the way the host does, and grants the master bypass for the master tenant, which the
    /// host grants only after the session check (SEC-AUTHZ-13).
    /// </summary>
    public static void SetTenant(IServiceProvider services, ResolvedTenant tenant)
    {
        var current = services.GetRequiredService<CurrentTenant>();
        current.Set(tenant);

        if (tenant.IsMaster)
        {
            current.GrantMasterBypass();
        }
    }

    /// <summary>Adds a school with its own domain and returns its id.</summary>
    public static async Task<EntityIds> AddSchoolAsync(string name, string? domain)
    {
        var school = await NewSchoolAsync(name, domain);
        await AddAsync(school);
        await GrantDefaultModulesAsync(school.Id);
        return new EntityIds(school.Id, school.PublicId);
    }

    /// <summary>A school in its own new customer tenant, as a registration creates them (ADR-021).</summary>
    private static async Task<School> NewSchoolAsync(string name, string? domain)
    {
        var tenant = Tenant.Create(name);
        await AddAsync(tenant);
        var school = School.Create(name, domain);
        school.AssignToTenant(tenant.Id);
        return school;
    }

    /// <summary>Gives a school the modules that are on by default, as a registering school gets them (CreateSchool).</summary>
    private static async Task GrantDefaultModulesAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        await SchoolDefaultModules.GrantAsync(context, schoolId, DateTimeOffset.UtcNow, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Adds a country with its regional settings and returns its id.</summary>
    public static async Task<EntityIds> AddCountryAsync(string code, string name, string[] languages, string defaultLanguage, string timeZoneId, string currencyCode)
    {
        var country = Country.Create(code, name, languages, defaultLanguage, timeZoneId, currencyCode);
        await AddAsync(country);
        return new EntityIds(country.Id, country.PublicId);
    }

    /// <summary>Adds a school with its own domain in a country and returns its id.</summary>
    public static async Task<EntityIds> AddSchoolAsync(string name, string? domain, int countryId)
    {
        var school = await NewSchoolAsync(name, domain);
        school.AssignCountry(countryId);
        await AddAsync(school);
        await GrantDefaultModulesAsync(school.Id);
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
    /// trial of <paramref name="trialDays"/> days, at <paramref name="amount"/> a month; with a campus, the modules are for that campus only. The school's cached
    /// entitlements are dropped, as after a real change.
    /// </summary>
    public static async Task<Subscription> SubscribeAsync(int schoolId, ModuleCode[] moduleCodes, DateTimeOffset? start = null, int? campusId = null, int trialDays = 0, decimal amount = 0m)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        SetTenant(scope.ServiceProvider, Resolve(MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var book = await context.PriceBooks.FirstOrDefaultAsync(b => b.CountryCode == "XX");
        if (book is null)
        {
            book = PriceBook.Create("XX", "XXX");
            context.PriceBooks.Add(book);
            await context.SaveChangesAsync();
        }

        // Without a campus the modules are for the whole school: they go on its main campus.
        var campus = campusId is null
            ? await context.Campuses.SingleAsync(b => b.SchoolId == schoolId && b.Kind == EducationPortal.Domain.Enums.CampusKind.Main)
            : await context.Campuses.FirstAsync(b => b.Id == campusId);
        var subscription = Subscription.Create(schoolId, null, book, BillingCycle.Monthly, amount, start ?? DateTimeOffset.UtcNow, trialDays);
        foreach (var module in await context.Modules.Where(m => moduleCodes.Contains(m.Code)).ToListAsync())
        {
            subscription.AddModule(module, campus);
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
        // The master school belongs to the master tenant (ADR-021).
        var masterTenant = Tenant.CreateMaster("Education Portal");
        await AddAsync(masterTenant);
        var master = School.CreateMaster("Education Portal", MasterDomain);
        master.AssignToTenant(masterTenant.Id);
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

    /// <summary>The internal id of a school's main campus, for a test that adds rows on it directly.</summary>
    public static async Task<int> MainCampusIdAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        SetTenant(scope.ServiceProvider, Resolve(MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Campuses
            .Where(b => b.SchoolId == schoolId && b.Kind == EducationPortal.Domain.Enums.CampusKind.Main)
            .Select(b => b.Id)
            .SingleAsync();
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
            SetTenant(scope.ServiceProvider, _tenant);
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
