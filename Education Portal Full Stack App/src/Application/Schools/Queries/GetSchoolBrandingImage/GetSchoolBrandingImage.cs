using EducationPortal.Application.Branding.Queries.GetBrandingImage;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Schools.Queries.GetSchoolBrandingImage;

/// <summary>
/// Returns any school's logo or favicon, so the master portal can show it on the school form. Master tenant
/// administrators only (SEC-AUTHZ-07). Returns <c>null</c> when the school uses the default image.
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolBrandingImageQuery(Guid SchoolId, BrandingImageKind Kind) : IRequest<BrandingImageDto?>;

public class GetSchoolBrandingImageQueryValidator : AbstractValidator<GetSchoolBrandingImageQuery>
{
    public GetSchoolBrandingImageQueryValidator()
    {
        RuleFor(q => q.SchoolId).NotEmpty();
        RuleFor(q => q.Kind).IsInEnum();
    }
}

public class GetSchoolBrandingImageQueryHandler : IRequestHandler<GetSchoolBrandingImageQuery, BrandingImageDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolBrandingImageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BrandingImageDto?> Handle(GetSchoolBrandingImageQuery request, CancellationToken cancellationToken)
    {
        return await _context.BrandingImages
            .AsNoTracking()
            .Where(i => i.Kind == request.Kind && _context.Schools.Any(s => s.Id == i.SchoolId && s.PublicId == request.SchoolId))
            .Select(i => new BrandingImageDto(i.ContentType, i.Content))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
