using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Web.Services;

/// <summary>
/// The school and tenant of the current request. <see cref="Infrastructure.TenantResolution"/> sets them once, from the host,
/// before any endpoint runs (SEC-AUTHZ-04). The only other caller is a scheduled job, which has no request and sets the
/// master tenant on its own scope (<see cref="Jobs.SubscriptionLifecycleJob"/>). Nothing else may set them.
/// </summary>
public sealed class CurrentTenant : ICurrentTenant
{
    private int? _schoolId;
    private int? _tenantId;

    public bool IsResolved => _schoolId is not null;

    /// <inheritdoc />
    public int SchoolId => _schoolId ?? throw new InvalidOperationException("The tenant is not resolved for this scope.");

    /// <inheritdoc />
    public int TenantId => _tenantId ?? throw new InvalidOperationException("The tenant is not resolved for this scope.");

    public bool IsMaster { get; private set; }

    public bool BypassesTenantFilters { get; private set; }

    public void Set(ResolvedTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        if (IsResolved)
        {
            throw new InvalidOperationException("The tenant of a request is set once.");
        }

        _schoolId = tenant.SchoolId;
        _tenantId = tenant.TenantId;
        IsMaster = tenant.IsMaster;
    }

    /// <summary>
    /// Lets a signed-in master session see every tenant (SEC-AUTHZ-13). Only <see cref="Infrastructure.TenantResolution"/>
    /// calls it, after it has checked the session belongs to the master school on the master domain, and only the
    /// scheduled master job calls it on its own scope. Each call is audited where it is made.
    /// </summary>
    public void GrantMasterBypass()
    {
        if (!IsMaster)
        {
            throw new InvalidOperationException("Only the master tenant can bypass the tenant filters.");
        }

        BypassesTenantFilters = true;
    }

    /// <summary>
    /// The one allowed second change: a request on the master tenant's domain whose session belongs to a school without a
    /// domain of its own acts as that school, and no longer as the master tenant (so no master power, and the school's own
    /// branding, language, modules and rows). Only <see cref="Infrastructure.TenantResolution"/> calls it, after it has
    /// checked the signed session and that the school still has no domain.
    /// </summary>
    public void ActAsSchoolWithoutDomain(ResolvedTenant school)
    {
        ArgumentNullException.ThrowIfNull(school);

        if (!IsMaster || school.IsMaster)
        {
            throw new InvalidOperationException("Only a request on the master tenant's domain can act as a school without a domain.");
        }

        _schoolId = school.SchoolId;
        _tenantId = school.TenantId;
        IsMaster = false;
        BypassesTenantFilters = false;
    }
}
