using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Branding.Queries.GetBranding;

/// <summary>
/// Returns the branding of the school that owns the request's domain. It runs before sign-in, so the login page is
/// already themed (SEC-AUTHN-01 exception, listed in the PR). The school comes only from the host (SEC-AUTHZ-04).
/// </summary>
public record GetBrandingQuery : IRequest<BrandingDto>;

public class GetBrandingQueryHandler : IRequestHandler<GetBrandingQuery, BrandingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetBrandingQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<BrandingDto> Handle(GetBrandingQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new NotFoundException("host", nameof(School));

        return await BrandingStore.ReadAsync(_context, schoolId, cancellationToken);
    }
}
