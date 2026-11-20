using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Campuses.Common;

/// <inheritdoc cref="ICampusAccess"/>
/// <remarks>Scoped: the answer is worked out once per request.</remarks>
public sealed class CampusAccess : ICampusAccess
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ICurrentTenant _tenant;
    private readonly IIdentityService _identityService;

    private bool _loaded;
    private IReadOnlyCollection<int>? _allowed;

    public CampusAccess(IApplicationDbContext context, IUser user, ICurrentTenant tenant, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _tenant = tenant;
        _identityService = identityService;
    }

    public async Task<IReadOnlyCollection<int>?> GetAllowedCampusIdsAsync(CancellationToken cancellationToken)
    {
        if (_loaded)
        {
            return _allowed;
        }

        _allowed = await LoadAsync(cancellationToken);
        _loaded = true;

        return _allowed;
    }

    public async Task EnsureCampusAsync(int? campusId, CancellationToken cancellationToken)
    {
        if (campusId is not { } id)
        {
            return;
        }

        var allowed = await GetAllowedCampusIdsAsync(cancellationToken);
        if (allowed is not null && !allowed.Contains(id))
        {
            throw new ForbiddenAccessException();
        }
    }

    private async Task<IReadOnlyCollection<int>?> LoadAsync(CancellationToken cancellationToken)
    {
        if (_tenant.IsMaster || (_user.Roles?.Contains(Roles.Administrator) ?? false))
        {
            return null;
        }

        if (_user.Id is not { } userId || _tenant.SchoolId is not { } schoolId)
        {
            return [];
        }

        var personId = await _identityService.FindPersonIdOfAccountAsync(userId);
        if (personId is null)
        {
            return [];
        }

        return await _context.UserCampuses
            .AsNoTracking()
            .Where(u => u.SchoolId == schoolId && u.UserDetailsId == personId)
            .Select(u => u.CampusId)
            .ToListAsync(cancellationToken);
    }
}
