using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using EducationPortal.Infrastructure.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>The cloud-agnostic secrets layer (ADR-006).</summary>
public static class SecretsDependencyInjection
{
    /// <summary>The store without the cache, for the health check.</summary>
    public const string UncachedProviderKey = "secrets-uncached";

    /// <summary>
    /// Platform secrets the app needs before its services are built, and the configuration key each one fills.
    /// </summary>
    public static readonly IReadOnlyDictionary<SecretKey, string> StartupSecrets = new Dictionary<SecretKey, string>
    {
        [SecretKey.Platform("db-connection")] = $"ConnectionStrings:{Services.Database}",
    };

    /// <summary>
    /// Registers the store that <c>Secrets:Provider</c> selects, wrapped in retries, the cache and tracing, as
    /// <see cref="ISecretProvider"/>, and <see cref="ITenantSecretProvider"/> per request. With no provider set,
    /// any secret use fails with a clear message and the app runs on normal configuration.
    /// </summary>
    public static IServiceCollection AddSecretsManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(SecretsOptions.SectionName);

        services.AddOptions<SecretsOptions>().Bind(section).ValidateOnStart();
        services.AddSingleton<IValidateOptions<SecretsOptions>, SecretsOptionsValidator>();
        services.AddScoped<ITenantSecretProvider, TenantSecretProvider>();

        if (ReadOptions(configuration).Kind == SecretProviderKind.None)
        {
            services.AddSingleton<ISecretProvider, NotConfiguredSecretProvider>();
            return services;
        }

        services.AddMemoryCache();

        services.AddKeyedSingleton<ISecretProvider>(UncachedProviderKey, (provider, _) =>
            new ResilientSecretProvider(
                SecretProviderFactory.Create(provider.GetRequiredService<IOptions<SecretsOptions>>().Value),
                ResilientSecretProvider.CreatePipeline(provider.GetRequiredService<ILogger<ResilientSecretProvider>>())));

        services.AddMetrics();
        services.TryAddSingleton<SecretsTelemetry>();

        // Spans wrap the cache, so each span says whether the value came from the cache (ADR-007).
        services.AddSingleton<ISecretProvider>(provider => new InstrumentedSecretProvider(
            new CachedSecretProvider(
                provider.GetRequiredKeyedService<ISecretProvider>(UncachedProviderKey),
                provider.GetRequiredService<IMemoryCache>(),
                provider.GetRequiredService<IOptions<SecretsOptions>>(),
                provider.GetRequiredService<SecretsTelemetry>()),
            provider.GetRequiredService<IOptions<SecretsOptions>>().Value.Kind?.ToString() ?? "unknown"));

        services.AddHealthChecks().Add(new HealthCheckRegistration(
            SecretProviderHealthCheck.Name,
            provider => new SecretProviderHealthCheck(provider.GetRequiredKeyedService<ISecretProvider>(UncachedProviderKey)),
            failureStatus: null,
            tags: null));

        return services;
    }

    /// <summary>
    /// Loads <see cref="StartupSecrets"/> (such as <c>platform/db-connection</c>) from the store into configuration,
    /// before the DbContext is registered. A value found in the store wins over other configuration. When
    /// <c>Secrets:Provider</c> is empty, or a secret is missing, normal configuration is used, so the app still runs
    /// without a store. Invalid settings stop startup with a clear message.
    /// </summary>
    public static async Task AddPlatformSecretsAsync(this IHostApplicationBuilder builder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = ReadOptions(builder.Configuration);
        var errors = SecretsOptionsValidator.GetErrors(options);
        if (errors.Count > 0)
        {
            throw new OptionsValidationException(SecretsOptions.SectionName, typeof(SecretsOptions), errors);
        }

        if (options.Kind == SecretProviderKind.None)
        {
            return;
        }

        var store = SecretProviderFactory.Create(options);
        try
        {
            var provider = new ResilientSecretProvider(store, ResilientSecretProvider.CreatePipeline(NullLogger.Instance));
            var values = new Dictionary<string, string?>();

            foreach (var (key, configurationKey) in StartupSecrets)
            {
                if (await provider.GetSecretAsync(key, cancellationToken) is { } value)
                {
                    values[configurationKey] = value;
                }
            }

            if (values.Count > 0)
            {
                builder.Configuration.AddInMemoryCollection(values);
            }
        }
        finally
        {
            (store as IDisposable)?.Dispose();
        }
    }

    private static SecretsOptions ReadOptions(IConfiguration configuration) =>
        configuration.GetSection(SecretsOptions.SectionName).Get<SecretsOptions>() ?? new SecretsOptions();
}
