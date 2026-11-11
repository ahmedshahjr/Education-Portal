using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Infrastructure.Secrets;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>Tenant secrets are always scoped to the school from the request's host (SEC-AUTHZ-04, ADR-006).</summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TenantSecretProviderTests
{
    private readonly InMemorySecretProvider _store = new();

    [Test]
    public async Task ShouldStoreUnderTheCurrentTenantsKey()
    {
        await For(5).SetAsync("sms-api-key", "value-5", CancellationToken.None);

        _store.Values.ShouldContainKeyAndValue("tenants/5/sms-api-key", "value-5");
        (await For(5).GetAsync("sms-api-key", CancellationToken.None)).ShouldBe("value-5");
    }

    [Test]
    public async Task TenantAShouldNotReadTenantBsSecret()
    {
        await For(2).SetAsync("sms-api-key", "value-of-school-2", CancellationToken.None);

        (await For(1).GetAsync("sms-api-key", CancellationToken.None)).ShouldBeNull();
    }

    [Test]
    public async Task TenantAShouldNotDeleteTenantBsSecret()
    {
        await For(2).SetAsync("sms-api-key", "value-of-school-2", CancellationToken.None);

        await For(1).DeleteAsync("sms-api-key", CancellationToken.None);

        (await For(2).GetAsync("sms-api-key", CancellationToken.None)).ShouldBe("value-of-school-2");
    }

    [Test]
    public async Task ShouldThrowWhenNoTenantIsResolved()
    {
        var provider = For(null);

        (await Should.ThrowAsync<InvalidOperationException>(() => provider.GetAsync("sms-api-key", CancellationToken.None)))
            .Message.ShouldBe(TenantSecretProvider.NoTenantMessage);
        await Should.ThrowAsync<InvalidOperationException>(() => provider.SetAsync("sms-api-key", "v", CancellationToken.None));
        await Should.ThrowAsync<InvalidOperationException>(() => provider.DeleteAsync("sms-api-key", CancellationToken.None));
        _store.Values.ShouldBeEmpty();
    }

    [TestCase("../2/sms-api-key")]
    [TestCase("2/sms-api-key")]
    [TestCase("tenants/2/sms-api-key")]
    [TestCase("")]
    public async Task ShouldRejectNamesThatTryToReachAnotherKey(string name)
    {
        await Should.ThrowAsync<ArgumentException>(() => For(1).GetAsync(name, CancellationToken.None));
    }

    [Test]
    public void ShouldNotAcceptATenantIdFromTheCaller()
    {
        var parameters = typeof(ITenantSecretProvider).GetMethods().SelectMany(method => method.GetParameters());

        parameters.ShouldAllBe(parameter => parameter.ParameterType == typeof(string) || parameter.ParameterType == typeof(CancellationToken));
        parameters.ShouldNotContain(parameter => parameter.Name!.Contains("tenant", StringComparison.OrdinalIgnoreCase));
    }

    private TenantSecretProvider For(int? schoolId) => new(new FakeCurrentTenant(schoolId), _store);
}
