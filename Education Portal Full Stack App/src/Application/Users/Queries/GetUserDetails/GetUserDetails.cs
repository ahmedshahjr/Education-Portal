using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Queries.GetUserDetails;

/// <summary>
/// Lists the current school's people, optionally only one user type (for example the principals). The school is the one the
/// request's host resolves to (SEC-AUTHZ-04). Needs the Staff module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record GetUserDetailsQuery : IRequest<IReadOnlyList<UserDetailsDto>>
{
    /// <summary>When set, only the people of this type are listed.</summary>
    public UserType? UserType { get; init; }
}

public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, IReadOnlyList<UserDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetUserDetailsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<UserDetailsDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var query = _context.UserDetails
            .AsNoTracking()
            .Where(u => u.SchoolId == schoolId);

        if (request.UserType is { } userType)
        {
            query = query.Where(u => u.UserType == userType);
        }

        return await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserDetailsDto(u.PublicId, u.FirstName, u.MiddleName, u.LastName, u.UserType, u.Phone))
            .ToListAsync(cancellationToken);
    }
}
