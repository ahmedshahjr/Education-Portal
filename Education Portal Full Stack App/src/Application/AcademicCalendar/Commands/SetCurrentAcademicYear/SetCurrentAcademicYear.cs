using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.SetCurrentAcademicYear;

/// <summary>
/// Makes an academic year the school's current year; the previous current year stops being current in the same save. A
/// closed year cannot be made current. Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Edit)]
public record SetCurrentAcademicYearCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The year's public id, taken from the route.</summary>
    public Guid AcademicYearId { get; init; }
}

public class SetCurrentAcademicYearCommandValidator : AbstractValidator<SetCurrentAcademicYearCommand>
{
    public SetCurrentAcademicYearCommandValidator()
    {
        RuleFor(c => c.AcademicYearId).NotEmpty();
    }
}

public class SetCurrentAcademicYearCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public SetCurrentAcademicYearCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(SetCurrentAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.PublicId == request.AcademicYearId, cancellationToken);
        Guard.Against.NotFound(request.AcademicYearId, year);
        AcademicYearChecks.EnsureOpen(year);

        await _context.ExecuteInTransactionAsync(
            async ct =>
            {
                await AcademicYearChecks.MakeCurrentAsync(_context, year, ct);
                return true;
            },
            cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(AcademicYear),
            targetId: year.Id.ToString(CultureInfo.InvariantCulture));
    }
}
