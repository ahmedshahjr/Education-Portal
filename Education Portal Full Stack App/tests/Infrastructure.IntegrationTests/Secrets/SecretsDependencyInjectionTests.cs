using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Infrastructure.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>AddSecretsManagement and AddPlatformSecretsAsync (ADR-006).</summary>
public class SecretsDependencyInjectionTests
{
    [Test]
    public async Task WithoutAProviderSecretUseShouldFailClosed()
    {
        await using var services = Build([]);

        var provider = services.GetRequiredService<ISecretProvider>();

        provider.ShouldBeOfType<NotConfiguredSecretProvider>();
        await Should.ThrowAsync<InvalidOperationException>(
            () => provider.GetSecretAsync(Application.Common.Secrets.SecretKey.Platform("x"), CancellationToken.None));
    }

    [Test]
    public async Task ShouldWrapTheSelectedStoreInTheCacheAndRetries()
    {
        await using var services = Build(new()
        {
            ["Secrets:Provider"] = "HashiCorp",
            ["Secrets:HashiCorp:Address"] = "http://localhost:8200",
            ["Secrets:HashiCorp:Token"] = "test-token",
        });

        services.GetRequiredService<ISecretProvider>().ShouldBeOfType<CachedSecretProvider>();
        services.GetRequiredKeyedService<ISecretProvider>(SecretsDependencyInjection.UncachedProviderKey)
            .ShouldBeOfType<ResilientSecretProvider>()
            .Inner.ShouldBeOfType<HashiCorpVaultSecretProvider>();
    }

    [Test]
    public async Task ShouldRegisterTheTenantProviderPerRequest()
    {
        await using var services = Build([], tenant: 4);

        using var first = services.CreateScope();
        using var second = services.CreateScope();

        first.ServiceProvider.GetRequiredService<ITenantSecretProvider>()
            .ShouldNotBeSameAs(second.ServiceProvider.GetRequiredService<ITenantSecretProvider>());
    }

    [Test]
    public async Task InvalidSettingsShouldFailOnStartValidation()
    {
        await using var services = Build(new() { ["Secrets:Provider"] = "AzureKeyVault" });

        Should.Throw<OptionsValidationException>(() => services.GetRequiredService<IOptions<SecretsOptions>>().Value)
            .Message.ShouldContain("Secrets:AzureKeyVault:VaultUri");
    }

    [Test]
    public async Task PlatformSecretsWithoutAProviderShouldKeepNormalConfiguration()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:EducationPortalDb"] = "Host=normal",
        });

        await builder.AddPlatformSecretsAsync();

        builder.Configuration.GetConnectionString("EducationPortalDb").ShouldBe("Host=normal");
    }

    [Test]
    public async Task PlatformSecretsShouldFailFastOnMissingSettings()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Secrets:Provider"] = "HashiCorp",
            ["Secrets:HashiCorp:Address"] = "http://localhost:8200",
        });

        var exception = await Should.ThrowAsync<OptionsValidationException>(() => builder.AddPlatformSecretsAsync());

        exception.Message.ShouldContain("Secrets:HashiCorp:Token is missing");
    }

    private static ServiceProvider Build(Dictionary<string, string?> settings, int? tenant = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant(tenant));
        services.AddSecretsManagement(configuration);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
