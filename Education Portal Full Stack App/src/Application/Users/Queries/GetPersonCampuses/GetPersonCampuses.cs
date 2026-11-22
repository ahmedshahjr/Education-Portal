using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Queries.GetPersonCampuses;

/// <summary>The public ids of the campuses a person of the school is assigned to. Needs the Users &amp; Roles module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record GetPersonCampusesQuery
{
    public Guid PersonId { get; init; }
}

public class GetPersonCampusesQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetPersonCampusesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Guid>> Handle(GetPersonCampusesQuery request, CancellationToken cancellationToken)
    {
        // The tenant filter hides another school's person, so their id is not found (SEC-AUTHZ-03).
        var personId = await _context.UserDetails.ResolveIdAsync(request.PersonId, cancellationToken);

        return await _context.UserCampuses
            .AsNoTracking()
            .Where(u => u.UserDetailsId == personId)
            .Join(_context.Campuses.AsNoTracking(), u => u.CampusId, c => c.Id, (u, c) => new { c.PublicId, c.Code })
            .OrderBy(x => x.Code)
            .Select(x => x.PublicId)
            .ToListAsync(cancellationToken);
    }
}
