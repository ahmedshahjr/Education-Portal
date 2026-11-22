using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotoContent;

/// <summary>
/// Reads one gallery photo's image. Returns <c>null</c> when the photo is not this school's (or was deleted), so the caller
/// answers 404. Needs the School module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetSchoolPhotoContentQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public Guid PhotoId { get; init; }
}

public class GetSchoolPhotoContentQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetSchoolPhotoContentQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<SchoolPhotoContentDto?> Handle(GetSchoolPhotoContentQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        return await _context.SchoolPhotos
            .AsNoTracking()
            .Where(p => p.SchoolId == schoolId && p.PublicId == request.PhotoId)
            .Select(p => new SchoolPhotoContentDto(p.Content, p.ContentType, p.Version))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
