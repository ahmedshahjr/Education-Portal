using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotos;

/// <summary>
/// Lists a school's gallery photos in their order, without the image bytes (fetched one at a time). Needs the School module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetSchoolPhotosQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetSchoolPhotosQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetSchoolPhotosQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<SchoolPhotoDto>> Handle(GetSchoolPhotosQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        // The projection leaves out Content and Version is a short hash, so a list never loads the bytes.
        var photos = await _context.SchoolPhotos
            .AsNoTracking()
            .Where(p => p.SchoolId == schoolId)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new { p.PublicId, p.CampusId, p.Kind, p.Caption, p.SortOrder, p.ContentType, p.Version })
            .ToListAsync(cancellationToken);

        var campusIds = await CampusPublicIds.ForSchoolAsync(_context, schoolId, cancellationToken);

        return photos
            .Select(p => new SchoolPhotoDto(
                p.PublicId,
                p.CampusId is { } campus && campusIds.TryGetValue(campus, out var publicId) ? publicId : null,
                p.Kind,
                p.Caption,
                p.SortOrder,
                p.ContentType,
                p.Version))
            .ToList();
    }
}
