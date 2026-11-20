using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Common.Models;

/// <summary>
/// Decides which school a school-scoped use case acts on (AGENTS.md section 2, SEC-AUTHZ-04). A school user always acts on
/// the school its host resolves to; naming another school, by its public id, is a 404 so the other school is not revealed.
/// The master tenant owns no school and must name the school it acts on. The master tenant bypasses the global query
/// filters, so its handlers must also filter on the returned school id; this helper never changes those filters.
/// </summary>
public static class SchoolScope
{
    /// <summary>Returns the internal id of the school the request acts on.</summary>
    /// <param name="requestedSchoolId">The school's public id from the request, or <c>null</c> when none was sent.</param>
    public static async Task<int> ResolveAsync(
        IApplicationDbContext context,
        ICurrentTenant tenant,
        Guid? requestedSchoolId,
        CancellationToken cancellationToken)
    {
        if (tenant.IsMaster)
        {
            // The master tenant has no school of its own: it must say which school it means.
            if (requestedSchoolId is not { } masterChoice)
            {
                throw new NotFoundException(string.Empty, nameof(School));
            }

            return await context.Schools.ResolveIdAsync(masterChoice, cancellationToken);
        }

        var ownSchoolId = tenant.SchoolId ?? throw new ForbiddenAccessException();
        if (requestedSchoolId is not { } requested)
        {
            return ownSchoolId;
        }

        // Another school's id is reported as not found, the same as an id that does not exist.
        var resolved = await context.Schools.ResolveIdAsync(requested, cancellationToken);
        return resolved == ownSchoolId ? ownSchoolId : throw new NotFoundException(requested.ToString(), nameof(School));
    }
}
