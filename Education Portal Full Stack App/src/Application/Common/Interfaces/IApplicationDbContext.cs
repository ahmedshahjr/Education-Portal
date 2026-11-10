using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>The tenants. Handlers always scope reads to the current tenant or to a master-only use case.</summary>
    DbSet<School> Schools { get; }

    /// <summary>Logos and favicons. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    DbSet<BrandingImage> BrandingImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
