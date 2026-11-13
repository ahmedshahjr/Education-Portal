using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Modules.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace EducationPortal.Infrastructure.Data;

/// <inheritdoc cref="IModuleEntitlementStore"/>
/// <remarks>
/// An in-memory cache per server instance: invalidation reaches this instance at once and other instances when their
/// copy expires, so a module granted or removed takes effect everywhere within <see cref="TimeToLive"/>. Only ids and
/// module codes are cached, never personal data.
/// </remarks>
public sealed class ModuleEntitlementStore : IModuleEntitlementStore
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _cache;
    private readonly object _gate = new();
    private CancellationTokenSource _allEntries = new();

    public ModuleEntitlementStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<ModuleEntitlements> GetAsync(int schoolId, Func<CancellationToken, Task<ModuleEntitlements>> load, CancellationToken cancellationToken)
    {
        var key = Key(schoolId);

        if (_cache.TryGetValue(key, out ModuleEntitlements? cached) && cached is not null)
        {
            return cached;
        }

        // Taken before the read, so an invalidation that lands while loading discards this (possibly stale) entry.
        var allEntries = CurrentToken();
        var loaded = await load(cancellationToken);

        using var entry = _cache.CreateEntry(key);
        entry.Value = loaded;
        entry.AbsoluteExpirationRelativeToNow = TimeToLive;
        entry.AddExpirationToken(allEntries);

        return loaded;
    }

    public void Invalidate(int schoolId) => _cache.Remove(Key(schoolId));

    public void InvalidateAll()
    {
        CancellationTokenSource old;
        lock (_gate)
        {
            old = _allEntries;
            _allEntries = new CancellationTokenSource();
        }

        old.Cancel();
        old.Dispose();
    }

    private IChangeToken CurrentToken()
    {
        lock (_gate)
        {
            return new CancellationChangeToken(_allEntries.Token);
        }
    }

    private static string Key(int schoolId) => $"module-entitlements:{schoolId}";
}
