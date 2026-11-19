using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Web.Services;

/// <summary>
/// The tenant of the current request. <see cref="Infrastructure.TenantResolution"/> sets it once, from the host, before
/// any endpoint runs (SEC-AUTHZ-04). The only other caller is a scheduled job, which has no request and sets the master
/// tenant on its own scope (<see cref="Jobs.SubscriptionLifecycleJob"/>). Nothing else may set it.
/// </summary>
public sealed class CurrentTenant : ICurrentTenant
{
    public int? SchoolId { get; private set; }

    public bool IsMaster { get; private set; }

    public void Set(ResolvedTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        if (SchoolId is not null)
        {
            throw new InvalidOperationException("The tenant of a request is set once.");
        }

        SchoolId = tenant.SchoolId;
        IsMaster = tenant.IsMaster;
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

        SchoolId = school.SchoolId;
        IsMaster = false;
    }
}
