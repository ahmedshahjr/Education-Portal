using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Infrastructure.Data.Tenancy;

/// <summary>
/// An explicit, short system bypass of the tenant isolation, for work that has no request and belongs to no school: the
/// development seed that creates the master tenant. It is scoped to one unit of work and disposed when the work ends, and
/// it is not reachable from any endpoint. A scheduled job that acts as the master tenant sets the master tenant on its own
/// scope instead (SEC-AUTHZ-13).
/// </summary>
public sealed class TenantSystemScope : ITenantSystemScope
{
    private int _depth;

    /// <summary>True while a system scope is open on this unit of work.</summary>
    public bool IsActive => _depth > 0;

    /// <summary>Opens the bypass. Dispose the returned value to close it; scopes nest.</summary>
    public IDisposable Enter()
    {
        _depth++;
        return new Leave(this);
    }

    private sealed class Leave(TenantSystemScope owner) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner._depth--;
        }
    }
}
