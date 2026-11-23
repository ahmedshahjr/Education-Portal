using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Infrastructure.Identity;

/// <summary>Looks up the school that owns a host name, using the unique index on <see cref="School.Domain"/>.</summary>
internal sealed class TenantResolver(ApplicationDbContext context) : ITenantResolver
{
    public async Task<ResolvedTenant?> ResolveAsync(string host, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Length > School.DomainMaxLength)
        {
            return null;
        }

        var domain = host.Trim().TrimEnd('.').ToLowerInvariant();

        return await context.Schools
            .AsNoTracking()
            // A deleted school is already hidden by the query filter; an inactive one is refused here, so none of its
            // users can use the host.
            .Where(s => s.Domain == domain && s.IsActive)
            .Select(s => new ResolvedTenant(s.Id, s.TenantId, s.IsMaster))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ResolvedTenant?> ResolveSchoolWithoutDomainAsync(int schoolId, CancellationToken cancellationToken)
    {
        return await context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId && s.Domain == null && !s.IsMaster && s.IsActive)
            .Select(s => new ResolvedTenant(s.Id, s.TenantId, false))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
