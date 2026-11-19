using EducationPortal.Application.Audit.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Audit.Queries.GetChangeAudit;

/// <summary>
/// The latest recorded changes of the school, newest first. Read only, for the school's administrators. The change audit
/// is filtered by school in the database (a school never sees another school's entries), and the master tenant sees all.
/// </summary>
[Authorize(Roles = Roles.Administrator)]
public record GetChangeAuditQuery : IRequest<IReadOnlyList<ChangeAuditDto>>
{
    public int Take { get; init; } = 100;
}

public class GetChangeAuditQueryValidator : AbstractValidator<GetChangeAuditQuery>
{
    public GetChangeAuditQueryValidator()
    {
        RuleFor(q => q.Take).InclusiveBetween(1, 200);
    }
}

public class GetChangeAuditQueryHandler : IRequestHandler<GetChangeAuditQuery, IReadOnlyList<ChangeAuditDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChangeAuditQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ChangeAuditDto>> Handle(GetChangeAuditQuery request, CancellationToken cancellationToken)
    {
        return await _context.ChangeAuditEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.Id)
            .Take(request.Take)
            .Select(e => new ChangeAuditDto(e.OccurredAt, e.ActorUserId, e.EntityType, e.EntityId, e.Action, e.Changes))
            .ToListAsync(cancellationToken);
    }
}
