using EducationPortal.Application.Common.Secrets;
using EducationPortal.Infrastructure.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>Cache hits, eviction on Set and Delete, and per-tenant entries (ADR-006).</summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CachedSecretProviderTests
{
    private static readonly SecretKey Key = SecretKey.ForTenant(1, "sms-api-key");

    private readonly InMemorySecretProvider _store = new();
    private MemoryCache _cache = null!;
    private CachedSecretProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _provider = new CachedSecretProvider(_store, _cache, Options.Create(new SecretsOptions()));
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    [Test]
    public async Task ShouldReadTheStoreOnceThenServeFromTheCache()
    {
        _store.Values[Key.Path] = "v1";

        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBe("v1");
        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBe("v1");

        _store.Gets.ShouldBe(1);
    }

    [Test]
    public async Task ShouldLoadLazilyAndNotAtConstruction()
    {
        _store.Gets.ShouldBe(0);
        await _provider.GetSecretAsync(Key, CancellationToken.None);
        _store.Gets.ShouldBe(1);
    }

    [Test]
    public async Task SetShouldEvictTheCachedValue()
    {
        _store.Values[Key.Path] = "v1";
        await _provider.GetSecretAsync(Key, CancellationToken.None);

        await _provider.SetSecretAsync(Key, "v2", CancellationToken.None);

        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBe("v2");
        _store.Gets.ShouldBe(2);
    }

    [Test]
    public async Task DeleteShouldEvictTheCachedValue()
    {
        _store.Values[Key.Path] = "v1";
        await _provider.GetSecretAsync(Key, CancellationToken.None);

        await _provider.DeleteSecretAsync(Key, CancellationToken.None);

        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBeNull();
    }

    [Test]
    public async Task ShouldNotCacheAMissingSecret()
    {
        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBeNull();
        _store.Values[Key.Path] = "added-outside-the-app";

        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBe("added-outside-the-app");
    }

    [Test]
    public async Task ShouldKeepEachTenantsEntrySeparate()
    {
        var other = SecretKey.ForTenant(2, Key.Name);
        _store.Values[Key.Path] = "school-1";
        _store.Values[other.Path] = "school-2";

        (await _provider.GetSecretAsync(Key, CancellationToken.None)).ShouldBe("school-1");
        (await _provider.GetSecretAsync(other, CancellationToken.None)).ShouldBe("school-2");
    }

    [TestCase(10)]
    [TestCase(1)]
    public async Task ShouldCacheForTheConfiguredTtl(int minutes)
    {
        using var cache = new RecordingMemoryCache();
        var provider = new CachedSecretProvider(_store, cache, Options.Create(new SecretsOptions { CacheTtlMinutes = minutes }));
        _store.Values[Key.Path] = "v1";

        await provider.GetSecretAsync(Key, CancellationToken.None);

        cache.Entries.ShouldHaveSingleItem().AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(minutes));
    }

    [Test]
    public void TheDefaultTtlShouldBeTenMinutes()
    {
        new SecretsOptions().CacheTtlMinutes.ShouldBe(10);
    }

    private sealed class RecordingMemoryCache : IMemoryCache
    {
        private readonly MemoryCache _inner = new(new MemoryCacheOptions());

        public List<ICacheEntry> Entries { get; } = [];

        public ICacheEntry CreateEntry(object key)
        {
            var entry = _inner.CreateEntry(key);
            Entries.Add(entry);
            return entry;
        }

        public void Remove(object key) => _inner.Remove(key);

        public bool TryGetValue(object key, out object? value) => _inner.TryGetValue(key, out value);

        public void Dispose() => _inner.Dispose();
    }
}
