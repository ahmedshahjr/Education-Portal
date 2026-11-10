using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Azure.Identity;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Infrastructure;
using EducationPortal.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddScoped<CurrentTenant>();
        builder.Services.AddScoped<ICurrentTenant>(provider => provider.GetRequiredService<CurrentTenant>());

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        builder.Services.AddProblemDetails(options =>
        {
            // The trace id lets support find the logs for a failed request (SEC-WEB-06).
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
        });

        // Enums travel by name (for example "Cyan"), which keeps the API readable and gives Angular named enums.
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<MultipartFormOperationTransformer>();
            options.AddSchemaTransformer<FormFileSchemaTransformer>();
        });

        builder.Services.AddAntiforgery(options =>
        {
            // The header that Angular sends the request token in (SEC-WEB-03).
            options.HeaderName = AntiforgeryTokenCookie.HeaderName;
            options.Cookie.Name = "__Host-Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        builder.Services.AddHsts(options =>
        {
            // One year, including subdomains (SEC-WEB-01).
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            var signInPermitLimit = builder.Configuration.GetValue<int>("RateLimiting:SignInPermitLimit", 5);

            // Sign-in attempts per client address and minute (SEC-WEB-07). Each account is protected separately by
            // the lockout in Identity.
            options.AddPolicy(RateLimitPolicies.SignIn, context => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = signInPermitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            // Do not advertise the server software (SEC-WEB-09), and cap request bodies at 1 MB (SEC-WEB-07).
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 1024 * 1024;
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
