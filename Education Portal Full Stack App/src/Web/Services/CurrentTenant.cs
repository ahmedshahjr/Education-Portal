using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Web.Services;

/// <summary>
/// The tenant of the current request. <see cref="Infrastructure.TenantResolution"/> sets it once, from the host, before
/// any endpoint runs (SEC-AUTHZ-04). Nothing else may set it.
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
}
