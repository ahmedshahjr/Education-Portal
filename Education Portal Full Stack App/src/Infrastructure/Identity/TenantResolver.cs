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
            .Where(s => s.Domain == domain)
            .Select(s => new ResolvedTenant(s.Id, s.IsMaster))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
