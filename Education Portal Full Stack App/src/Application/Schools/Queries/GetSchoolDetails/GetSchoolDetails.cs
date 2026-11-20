using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Queries.GetSchoolDetails;

/// <summary>
/// A school's registration and contact details. Its administrator reads their own school; the master tenant names the school.
/// Needs the School module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetSchoolDetailsQuery : IRequest<SchoolDetailsDto>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

/// <summary>A school's registration and contact details. The city is a public id with its state, for the dropdowns.</summary>
public sealed record SchoolDetailsDto(
    string? LocalName,
    string? RegistrationNumber,
    string? RegistrationAuthority,
    string? TaxNumber,
    string? Address,
    Guid? StateId,
    Guid? CityId,
    string? Phone,
    string? Email,
    string? Website,
    int AcademicYearStartMonth);

public class GetSchoolDetailsQueryHandler : IRequestHandler<GetSchoolDetailsQuery, SchoolDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetSchoolDetailsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<SchoolDetailsDto> Handle(GetSchoolDetailsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new
            {
                s.LocalName,
                s.RegistrationNumber,
                s.RegistrationAuthority,
                s.TaxNumber,
                s.Address,
                s.CityId,
                s.Phone,
                s.Email,
                s.Website,
                s.AcademicYearStartMonth,
            })
            .FirstAsync(cancellationToken);

        var place = school.CityId is { } cityId
            ? await _context.Cities
                .AsNoTracking()
                .Where(c => c.Id == cityId)
                .Join(_context.States, c => c.StateId, st => st.Id, (c, st) => new { CityId = c.PublicId, StateId = st.PublicId })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new SchoolDetailsDto(
            school.LocalName,
            school.RegistrationNumber,
            school.RegistrationAuthority,
            school.TaxNumber,
            school.Address,
            place?.StateId,
            place?.CityId,
            school.Phone,
            school.Email,
            school.Website,
            school.AcademicYearStartMonth);
    }
}
