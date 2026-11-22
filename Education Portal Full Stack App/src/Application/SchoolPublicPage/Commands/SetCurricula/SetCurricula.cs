using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.SetCurricula;

/// <summary>
/// Replaces the set of curricula a school shows on its public page. Each kind is listed at most once; an empty list
/// clears the set. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record SetCurriculaCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public IReadOnlyList<CurriculumKind> Kinds { get; init; } = [];
}

public class SetCurriculaCommandValidator : AbstractValidator<SetCurriculaCommand>
{
    public SetCurriculaCommandValidator()
    {
        RuleFor(c => c.Kinds).NotNull();

        RuleForEach(c => c.Kinds).IsInEnum();

        RuleFor(c => c.Kinds)
            .Must(kinds => kinds.Distinct().Count() == kinds.Count)
            .WithMessage(_ => ValidationMessages.CurriculumListedTwice)
            .When(c => c.Kinds is not null);
    }
}

public class SetCurriculaCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public SetCurriculaCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(SetCurriculaCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var current = await _context.SchoolCurricula
            .Where(c => c.SchoolId == schoolId)
            .ToListAsync(cancellationToken);

        var wanted = request.Kinds.ToHashSet();
        _context.SchoolCurricula.RemoveRange(current.Where(c => !wanted.Contains(c.Kind)));

        var existing = current.Select(c => c.Kind).ToHashSet();
        foreach (var kind in wanted.Where(k => !existing.Contains(k)))
        {
            _context.SchoolCurricula.Add(SchoolCurriculum.Create(schoolId, kind));
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCurriculaChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolCurriculum),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
