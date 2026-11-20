using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.CreateTerm;

/// <summary>
/// Adds a term to an academic year that is not closed. Its dates fall inside the year, and its number and dates do not
/// clash with the year's other terms. Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Create)]
public record CreateTermCommand : IRequest<Guid>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The year's public id, taken from the route.</summary>
    public Guid AcademicYearId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Sequence { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }
}

public class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(c => c.AcademicYearId).NotEmpty();
        TermValidation.Fields(this, c => c.Name, c => c.Sequence, c => c.StartDate, c => c.EndDate);
    }
}

public class CreateTermCommandHandler : IRequestHandler<CreateTermCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateTermCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.PublicId == request.AcademicYearId, cancellationToken);
        Guard.Against.NotFound(request.AcademicYearId, year);

        await TermChecks.EnsureFitsAsync(_context, year, null, request.Sequence, request.StartDate, request.EndDate, cancellationToken);

        var term = Term.Create(schoolId, year.Id, request.Name, request.Sequence, request.StartDate, request.EndDate);
        _context.Terms.Add(term);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Term),
            targetId: term.Id.ToString(CultureInfo.InvariantCulture));

        return term.PublicId;
    }
}
