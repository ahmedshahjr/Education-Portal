using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>The tenants. Handlers always scope reads to the current tenant or to a master-only use case.</summary>
    DbSet<School> Schools { get; }

    /// <summary>Logos and favicons. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    DbSet<BrandingImage> BrandingImages { get; }

    /// <summary>Public requests from schools that want to join. Only the master tenant decides them.</summary>
    DbSet<SchoolRequest> SchoolRequests { get; }

    /// <summary>Countries and their regional settings. Platform reference data that only the master tenant changes.</summary>
    DbSet<Country> Countries { get; }

    /// <summary>States and provinces of each country, for address dropdowns. Platform reference data, not owned by a school.</summary>
    DbSet<State> States { get; }

    /// <summary>Cities of each state, for address dropdowns. Platform reference data, not owned by a school.</summary>
    DbSet<City> Cities { get; }

    /// <summary>The module catalogue: what the platform sells. Platform reference data that only the master tenant changes.</summary>
    DbSet<PortalModule> Modules { get; }

    /// <summary>Which modules require which. Platform reference data that only the master tenant changes.</summary>
    DbSet<ModuleDependency> ModuleDependencies { get; }

    /// <summary>The master tenant's per-school module switches. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    DbSet<SchoolModule> SchoolModules { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
