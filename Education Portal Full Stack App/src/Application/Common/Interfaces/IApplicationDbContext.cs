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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
