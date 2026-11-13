using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Data.Locations;
using EducationPortal.Infrastructure.Data.Interceptors;
using EducationPortal.Infrastructure.Identity;
using EducationPortal.Infrastructure.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        // Every commercial change with its old and new values, in the same save (ADR-016).
        builder.Services.AddScoped<ISaveChangesInterceptor, ChangeAuditInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();
        builder.Services.AddScoped<LocationDataImporter>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IModuleEntitlementStore, ModuleEntitlementStore>();

        AddIdentity(builder.Services);
        ConfigureSessions(builder.Services);

        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddScoped<ISecurityAuditLog, SecurityAuditLog>();
        builder.Services.AddScoped<ITenantResolver, TenantResolver>();
        builder.Services.AddScoped<ITenantLocaleSettings, TenantLocaleSettings>();

        // One secret store for the environment, shared by all schools (ADR-006).
        builder.Services.AddSecretsManagement(builder.Configuration);
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        // Every endpoint needs an authenticated user unless it opts out explicitly (SEC-AUTHN-01).
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // SEC-AUTHN-03: a length rule and no composition rules. The minimum is 15 characters, because a
                // password can be the only factor for students and parents.
                options.Password.RequiredLength = PasswordRules.MinimumLength;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                // SEC-AUTHN-05: three failed sign-ins lock the account for 30 minutes.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<PasswordPolicyValidator>();
    }

    private static void ConfigureSessions(IServiceCollection services)
    {
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // Check the stamp on every request, so a signed-out or disabled user is rejected on the next request
            // (SEC-AUTHN-07, SEC-AUTHN-08).
            options.ValidationInterval = TimeSpan.Zero;
        });

        services.PostConfigureAll<CookieAuthenticationOptions>(options =>
        {
            // Applies to every cookie scheme that Identity adds (SEC-AUTHN-06).
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            // The __Host- prefix makes browsers require Secure, Path=/ and no Domain attribute, so the cookie stays
            // on one host and is never sent to another school's domain (SEC-AUTHN-06).
            options.Cookie.Name = "__Host-EducationPortal";
            options.Cookie.Path = "/";
            options.Cookie.IsEssential = true;

            // Idle timeout of 15 minutes that slides with each request (SEC-AUTHN-07).
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            options.SlidingExpiration = true;

            // The API answers with status codes instead of redirecting to a login page.
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            // Keep the handlers that Identity set, and add the 12-hour absolute lifetime (SEC-AUTHN-07).
            var previousSigningIn = options.Events.OnSigningIn;
            options.Events.OnSigningIn = async context =>
            {
                var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
                SessionLifetime.MarkSignedIn(context.Properties, now);

                if (previousSigningIn is not null)
                {
                    await previousSigningIn(context);
                }
            };

            var previousValidatePrincipal = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();

                if (!SessionLifetime.IsWithinAbsoluteLifetime(context.Properties, now))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return;
                }

                if (previousValidatePrincipal is not null)
                {
                    await previousValidatePrincipal(context);
                }
            };
        });
    }
}
