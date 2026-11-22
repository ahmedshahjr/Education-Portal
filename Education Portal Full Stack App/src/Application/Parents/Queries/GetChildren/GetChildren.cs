using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Parents.Common;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Parents.Queries.GetChildren;

/// <summary>The students linked to one parent of the school, for the school's staff. Needs the Staff module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record GetChildrenQuery
{
    public Guid ParentId { get; init; }
}

public class GetChildrenQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetChildrenQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserDetailsDto>> Handle(GetChildrenQuery request, CancellationToken cancellationToken)
    {
        var parentId = await _context.UserDetails.ResolveIdAsync(request.ParentId, cancellationToken);

        return await ParentChildren.ListAsync(_context, parentId, cancellationToken);
    }
}
