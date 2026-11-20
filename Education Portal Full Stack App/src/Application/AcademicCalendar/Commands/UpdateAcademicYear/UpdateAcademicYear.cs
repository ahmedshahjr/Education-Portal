using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.UpdateAcademicYear;

/// <summary>
/// Changes an academic year's name, dates and status. A closed year can only be reopened (a status other than Closed, with
/// the same or new values); closing a year stops it being current. The year's terms must still fall inside its dates.
/// Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Edit)]
public record UpdateAcademicYearCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The year's public id, taken from the route.</summary>
    public Guid AcademicYearId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public AcademicYearStatus Status { get; init; }
}

public class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(c => c.AcademicYearId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(AcademicYear.NameMaxLength);
        RuleFor(c => c.Status).IsInEnum();
        AcademicYearValidation.Dates(this, c => c.StartDate, c => c.EndDate);
    }
}

public class UpdateAcademicYearCommandHandler : IRequestHandler<UpdateAcademicYearCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateAcademicYearCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.PublicId == request.AcademicYearId, cancellationToken);
        Guard.Against.NotFound(request.AcademicYearId, year);

        var staysClosed = year.Status == AcademicYearStatus.Closed && request.Status == AcademicYearStatus.Closed;
        if (staysClosed)
        {
            // Nothing of a closed year can change; it must be reopened first.
            if (year.Name != request.Name.Trim() || year.StartDate != request.StartDate || year.EndDate != request.EndDate)
            {
                AcademicYearChecks.EnsureOpen(year);
            }

            return;
        }

        await AcademicYearChecks.EnsureFitsAsync(_context, schoolId, year.Id, request.Name, request.StartDate, request.EndDate, cancellationToken);

        var termOutside = await _context.Terms.AnyAsync(
            t => t.AcademicYearId == year.Id && (t.StartDate < request.StartDate || t.EndDate > request.EndDate),
            cancellationToken);
        if (termOutside)
        {
            throw CalendarRules.Invalid(nameof(request.StartDate), ValidationMessages.TermOutsideYear);
        }

        if (request.Status == AcademicYearStatus.Closed)
        {
            year.Change(request.Name, request.StartDate, request.EndDate);
            year.SetStatus(AcademicYearStatus.Closed);
        }
        else
        {
            year.SetStatus(request.Status);
            year.Change(request.Name, request.StartDate, request.EndDate);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(AcademicYear),
            targetId: year.Id.ToString(CultureInfo.InvariantCulture));
    }
}
