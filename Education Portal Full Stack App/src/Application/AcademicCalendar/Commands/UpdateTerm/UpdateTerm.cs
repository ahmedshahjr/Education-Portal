using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.UpdateTerm;

/// <summary>
/// Changes a term's name, number and dates. Its year must not be closed, and the same rules as for a new term apply.
/// Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Edit)]
public record UpdateTermCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The term's public id, taken from the route.</summary>
    public Guid TermId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Sequence { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }
}

public class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(c => c.TermId).NotEmpty();
        TermValidation.Fields(this, c => c.Name, c => c.Sequence, c => c.StartDate, c => c.EndDate);
    }
}

public class UpdateTermCommandHandler : IRequestHandler<UpdateTermCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateTermCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateTermCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var term = await _context.Terms.FirstOrDefaultAsync(t => t.SchoolId == schoolId && t.PublicId == request.TermId, cancellationToken);
        Guard.Against.NotFound(request.TermId, term);

        var year = await _context.AcademicYears.FirstAsync(y => y.Id == term.AcademicYearId, cancellationToken);
        await TermChecks.EnsureFitsAsync(_context, year, term.Id, request.Sequence, request.StartDate, request.EndDate, cancellationToken);

        term.Change(request.Name, request.Sequence, request.StartDate, request.EndDate);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Term),
            targetId: term.Id.ToString(CultureInfo.InvariantCulture));
    }
}
