namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// An explicit system bypass of the tenant isolation (ADR-026), for a unit of work that acts for no signed-in user and
/// creates rows of a school that the current request does not belong to: a self-service registration, or the
/// development seed. Open it only around that work, dispose it when the work ends, and never from an endpoint that reads
/// data for the caller.
/// </summary>
public interface ITenantSystemScope
{
    /// <summary>True while a system scope is open on this unit of work.</summary>
    bool IsActive { get; }

    /// <summary>Opens the bypass. Dispose the returned value to close it; scopes nest.</summary>
    IDisposable Enter();
}
