using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Queries.GetSchoolBranding;

/// <summary>Returns any school's branding, for the master portal. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolBrandingQuery(Guid SchoolId) : IRequest<BrandingDto>;

public class GetSchoolBrandingQueryValidator : AbstractValidator<GetSchoolBrandingQuery>
{
    public GetSchoolBrandingQueryValidator()
    {
        RuleFor(q => q.SchoolId).NotEmpty();
    }
}

public class GetSchoolBrandingQueryHandler : IRequestHandler<GetSchoolBrandingQuery, BrandingDto>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolBrandingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrandingDto> Handle(GetSchoolBrandingQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);

        return await BrandingStore.ReadAsync(_context, schoolId, cancellationToken);
    }
}
