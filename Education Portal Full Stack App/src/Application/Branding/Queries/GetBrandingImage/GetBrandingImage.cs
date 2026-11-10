using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Branding.Queries.GetBrandingImage;

/// <summary>
/// Returns the logo or favicon of the school that owns the request's domain. Public, like <see cref="GetBranding.GetBrandingQuery"/>,
/// because the login page and the browser tab show them before sign-in. Returns <c>null</c> when the school has none.
/// </summary>
public record GetBrandingImageQuery(BrandingImageKind Kind) : IRequest<BrandingImageDto?>;

public sealed record BrandingImageDto(string ContentType, byte[] Content);

public class GetBrandingImageQueryValidator : AbstractValidator<GetBrandingImageQuery>
{
    public GetBrandingImageQueryValidator()
    {
        RuleFor(q => q.Kind).IsInEnum();
    }
}

public class GetBrandingImageQueryHandler : IRequestHandler<GetBrandingImageQuery, BrandingImageDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetBrandingImageQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<BrandingImageDto?> Handle(GetBrandingImageQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new NotFoundException("host", nameof(School));

        return await _context.BrandingImages
            .AsNoTracking()
            .Where(i => i.SchoolId == schoolId && i.Kind == request.Kind)
            .Select(i => new BrandingImageDto(i.ContentType, i.Content))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
