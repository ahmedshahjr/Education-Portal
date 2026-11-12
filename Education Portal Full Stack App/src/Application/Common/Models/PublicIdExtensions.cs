using EducationPortal.Domain.Common;

namespace EducationPortal.Application.Common.Models;

/// <summary>
/// Turns the <see cref="Guid"/> that a request carries into the internal <c>int</c> key before any business logic
/// (ENG-ID-03). The lookup goes through the context's query filters, so a school can never resolve another school's
/// row (SEC-AUTHZ-03): an id from another tenant is simply not found.
/// </summary>
public static class PublicIdExtensions
{
    public static async Task<int> ResolveIdAsync<TEntity>(this IQueryable<TEntity> source, Guid publicId, CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        var ids = await source
            .Where(e => e.PublicId == publicId)
            .Select(e => e.Id)
            .Take(1)
            .ToListAsync(cancellationToken);

        return ids.Count == 1 ? ids[0] : throw new NotFoundException(publicId.ToString(), typeof(TEntity).Name);
    }
}
