using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolProfile;

/// <summary>Reads a school's public page text. Needs the School module (AGENTS.md section 2).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetSchoolProfileQuery : IRequest<SchoolProfileDto>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetSchoolProfileQueryHandler : IRequestHandler<GetSchoolProfileQuery, SchoolProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetSchoolProfileQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<SchoolProfileDto> Handle(GetSchoolProfileQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var profile = await _context.SchoolProfiles
            .AsNoTracking()
            .Where(p => p.SchoolId == schoolId)
            .Select(p => new SchoolProfileDto(
                p.Tagline,
                p.About,
                p.EstablishedYear,
                p.GenderPolicy,
                p.VisitingHours,
                p.VirtualTourUrl,
                p.IsPublished,
                p.PublishedAt))
            .SingleOrDefaultAsync(cancellationToken);

        // A school that has never saved its page reads as an empty, unpublished draft.
        return profile ?? new SchoolProfileDto(string.Empty, string.Empty, null, SchoolGenderPolicy.CoEducation, null, null, false, null);
    }
}
