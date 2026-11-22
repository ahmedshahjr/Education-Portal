using EducationPortal.Application.Audit.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Audit.Queries.GetChangeAudit;

/// <summary>
/// The recorded changes of the school, newest first, paged and filtered (ADR-023, R4.2). A school's administrators and the
/// master tenant see every change. Anyone else sees only the changes of modules where a role of theirs gives the
/// <see cref="AccessRight.ViewHistory"/> right; a change of a type that belongs to no module is never shown to them. The
/// tenant filter keeps the school's own entries (SEC-AUTHZ-03).
/// </summary>
[Authorize]
public record GetChangeAuditQuery
{
    public int Skip { get; init; }

    public int Take { get; init; } = 100;

    /// <summary>Only changes of this entity type (its class name, for example <c>Campus</c>), or null for all.</summary>
    public string? EntityType { get; init; }

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }
}

public class GetChangeAuditQueryValidator : AbstractValidator<GetChangeAuditQuery>
{
    public GetChangeAuditQueryValidator()
    {
        RuleFor(q => q.Skip).GreaterThanOrEqualTo(0);
        RuleFor(q => q.Take).InclusiveBetween(1, 200);
        RuleFor(q => q.EntityType).MaximumLength(100);
        RuleFor(q => q.To).GreaterThanOrEqualTo(q => q.From).When(q => q.From is not null && q.To is not null);
    }
}

public class GetChangeAuditQueryHandler
{
    /// <summary>The module whose ViewHistory right shows each entity type's changes to a non-administrator.</summary>
    public static readonly IReadOnlyDictionary<string, ModuleCode> ModuleOfEntity = new Dictionary<string, ModuleCode>(StringComparer.Ordinal)
    {
        ["Campus"] = ModuleCode.Campus,
        ["UserCampus"] = ModuleCode.Campus,
        ["UserDetails"] = ModuleCode.Staff,
        ["AccountRole"] = ModuleCode.UsersRoles,
        ["SchoolRole"] = ModuleCode.UsersRoles,
        ["RolePermission"] = ModuleCode.UsersRoles,
        ["ParentStudent"] = ModuleCode.ParentCommunication,
        ["Course"] = ModuleCode.AcademicSetup,
        ["Enrolment"] = ModuleCode.AcademicSetup,
        ["Assessment"] = ModuleCode.Exams,
        ["Score"] = ModuleCode.Exams,
        ["GradingScale"] = ModuleCode.Exams,
        ["GradingBand"] = ModuleCode.Exams,
        ["TimetableSlot"] = ModuleCode.Timetable,
        ["SchoolEvent"] = ModuleCode.School,
    };

    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ICurrentTenant _tenant;
    private readonly IRoleRights _roleRights;
    private readonly IIdentityService _identityService;

    public GetChangeAuditQueryHandler(IApplicationDbContext context, IUser user, ICurrentTenant tenant, IRoleRights roleRights, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _tenant = tenant;
        _roleRights = roleRights;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<ChangeAuditDto>> Handle(GetChangeAuditQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChangeAuditEntries.AsNoTracking();

        if (!SeesEverything())
        {
            var visibleTypes = await VisibleEntityTypesAsync(cancellationToken);
            if (visibleTypes.Count == 0)
            {
                return [];
            }

            query = query.Where(e => visibleTypes.Contains(e.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var entityType = request.EntityType.Trim();
            query = query.Where(e => e.EntityType == entityType);
        }

        if (request.From is { } from)
        {
            query = query.Where(e => e.OccurredAt >= from);
        }

        if (request.To is { } to)
        {
            query = query.Where(e => e.OccurredAt <= to);
        }

        var rows = await query
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(e => new ChangeAuditDto(e.OccurredAt, e.ActorUserId, e.EntityType, e.EntityId, e.Action, e.Changes))
            .ToListAsync(cancellationToken);

        // The names are read once per distinct actor of the page, not once per row.
        var names = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var actor in rows.Select(r => r.ActorUserId).Where(a => a is not null).Distinct())
        {
            names[actor!] = await _identityService.GetUserNameAsync(actor!);
        }

        return rows
            .Select(r => r with { ActorName = r.ActorUserId is { } id && names.TryGetValue(id, out var name) ? name : null })
            .ToList();
    }

    /// <summary>The master tenant and the school's administrators see every change (ADR-018).</summary>
    private bool SeesEverything() => _tenant.IsMaster || (_user.Roles?.Contains(Roles.Administrator) ?? false);

    /// <summary>The entity types whose module this user holds ViewHistory in. Fails closed: nothing without a school.</summary>
    private async Task<HashSet<string>> VisibleEntityTypesAsync(CancellationToken cancellationToken)
    {
        var visible = new HashSet<string>(StringComparer.Ordinal);
        if (_user.Id is not { } userId || _tenant.SchoolId is not { } schoolId)
        {
            return visible;
        }

        foreach (var module in ModuleOfEntity.Values.Distinct())
        {
            if (!await _roleRights.HasRightAsync(userId, schoolId, module, AccessRight.ViewHistory, cancellationToken))
            {
                continue;
            }

            foreach (var (entityType, owner) in ModuleOfEntity)
            {
                if (owner == module)
                {
                    visible.Add(entityType);
                }
            }
        }

        return visible;
    }
}
