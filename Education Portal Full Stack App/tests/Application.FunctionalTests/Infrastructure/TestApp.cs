using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
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
    public static int MasterSchoolId { get; private set; }

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
    public static async Task<int> AddSchoolAsync(string name, string domain)
    {
        var school = School.Create(name, domain);
        await AddAsync(school);
        return school.Id;
    }

    /// <summary>Adds a country with its regional settings and returns its id.</summary>
    public static async Task<int> AddCountryAsync(string code, string name, string[] languages, string defaultLanguage, string timeZoneId, string currencyCode)
    {
        var country = Country.Create(code, name, languages, defaultLanguage, timeZoneId, currencyCode);
        await AddAsync(country);
        return country.Id;
    }

    /// <summary>Adds a school with its own domain in a country and returns its id.</summary>
    public static async Task<int> AddSchoolAsync(string name, string domain, int countryId)
    {
        var school = School.Create(name, domain);
        school.AssignCountry(countryId);
        await AddAsync(school);
        return school.Id;
    }

    public static string? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

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

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;

        // Every request needs a tenant, so the master tenant is created again after each reset.
        var master = School.CreateMaster("Education Portal", MasterDomain);
        await AddAsync(master);
        MasterSchoolId = master.Id;
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

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

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
