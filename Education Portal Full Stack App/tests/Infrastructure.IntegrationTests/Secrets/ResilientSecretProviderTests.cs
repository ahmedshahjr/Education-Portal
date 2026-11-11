using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Secrets;
using EducationPortal.Infrastructure.Secrets;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>Retries of transient failures, and no secret value in logs or exceptions (SEC-LOG-02, ADR-006).</summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ResilientSecretProviderTests
{
    private const string SecretValue = "do-not-leak-this-value";

    private static readonly SecretKey Key = SecretKey.ForTenant(3, "sms-api-key");

    private readonly CapturingLogger _logger = new();

    [Test]
    public async Task ShouldRetryTransientFailures()
    {
        var store = new FlakySecretProvider(failures: 2, transient: true);

        (await Provider(store).GetSecretAsync(Key, CancellationToken.None)).ShouldBe("value");

        store.Calls.ShouldBe(3);
        _logger.Messages.Count(message => message.Contains("retry", StringComparison.Ordinal)).ShouldBe(2);
    }

    [Test]
    public async Task ShouldGiveUpAfterTheMaximumAttempts()
    {
        var store = new FlakySecretProvider(failures: 100, transient: true);

        await Should.ThrowAsync<SecretProviderException>(() => Provider(store).GetSecretAsync(Key, CancellationToken.None));

        store.Calls.ShouldBe(ResilientSecretProvider.MaxRetryAttempts + 1);
    }

    [Test]
    public async Task ShouldNotRetryPermanentFailures()
    {
        var store = new FlakySecretProvider(failures: 1, transient: false);

        await Should.ThrowAsync<SecretProviderException>(() => Provider(store).DeleteSecretAsync(Key, CancellationToken.None));

        store.Calls.ShouldBe(1);
    }

    [Test]
    public async Task ShouldKeepTheValueOutOfLogsAndExceptions()
    {
        var store = new FlakySecretProvider(failures: 100, transient: true);

        var exception = await Should.ThrowAsync<SecretProviderException>(
            () => Provider(store).SetSecretAsync(Key, SecretValue, CancellationToken.None));

        exception.ToString().ShouldNotContain(SecretValue);
        exception.Message.ShouldContain(Key.Path);
        _logger.Messages.ShouldNotBeEmpty();
        _logger.Messages.ShouldAllBe(message => !message.Contains(SecretValue));
    }

    private ResilientSecretProvider Provider(FlakySecretProvider store) =>
        new(store, ResilientSecretProvider.CreatePipeline(_logger, TimeSpan.Zero));
}
