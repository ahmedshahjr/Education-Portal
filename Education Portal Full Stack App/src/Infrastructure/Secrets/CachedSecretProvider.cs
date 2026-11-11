using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Keeps secret values in memory for <c>Secrets:CacheTtlMinutes</c> (default 10) so a request does not call the store
/// each time (ADR-006). Values are loaded on first use, never all at startup. The cache key is the logical key, which
/// includes the tenant id, so one school's entry is never served for another. Set and Delete evict the entry.
/// Missing secrets are not cached, so a secret added outside the app is seen on the next read.
/// </summary>
public sealed class CachedSecretProvider(ISecretProvider inner, IMemoryCache cache, IOptions<SecretsOptions> options)
    : ISecretProvider
{
    private const string CacheKeyPrefix = "EducationPortal.Secrets:";

    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(options.Value.CacheTtlMinutes);

    public async Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (cache.TryGetValue(CacheKey(key), out string? cached))
        {
            return cached;
        }

        var value = await inner.GetSecretAsync(key, cancellationToken);

        if (value is not null)
        {
            cache.Set(CacheKey(key), value, _ttl);
        }

        return value;
    }

    public async Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);

        await inner.SetSecretAsync(key, value, cancellationToken);
        cache.Remove(CacheKey(key));
    }

    public async Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);

        await inner.DeleteSecretAsync(key, cancellationToken);
        cache.Remove(CacheKey(key));
    }

    private static string CacheKey(SecretKey key) => CacheKeyPrefix + key.Path;
}
