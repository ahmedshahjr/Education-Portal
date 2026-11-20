using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.DeleteAcademicYear;

/// <summary>
/// Deletes an academic year that has no terms left. The row is kept (soft delete) and hidden everywhere; it stops being the
/// current year. Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Delete)]
public record DeleteAcademicYearCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The year's public id, taken from the route.</summary>
    public Guid AcademicYearId { get; init; }
}

public class DeleteAcademicYearCommandValidator : AbstractValidator<DeleteAcademicYearCommand>
{
    public DeleteAcademicYearCommandValidator()
    {
        RuleFor(c => c.AcademicYearId).NotEmpty();
    }
}

public class DeleteAcademicYearCommandHandler : IRequestHandler<DeleteAcademicYearCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteAcademicYearCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.PublicId == request.AcademicYearId, cancellationToken);
        Guard.Against.NotFound(request.AcademicYearId, year);

        if (await _context.Terms.AnyAsync(t => t.AcademicYearId == year.Id, cancellationToken))
        {
            throw CalendarRules.Invalid(nameof(request.AcademicYearId), ValidationMessages.AcademicYearHasTerms);
        }

        year.ClearCurrent();
        year.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(AcademicYear),
            targetId: year.Id.ToString(CultureInfo.InvariantCulture));
    }
}
