using EducationPortal.Domain.Enums;
using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.DeleteShift;

/// <summary>Deletes a shift. The row is kept (soft delete) and hidden everywhere. Needs the Academic setup module. Audited (SEC-LOG-03).</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Delete)]
public record DeleteShiftCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The shift's public id, taken from the route.</summary>
    public Guid ShiftId { get; init; }
}

public class DeleteShiftCommandValidator : AbstractValidator<DeleteShiftCommand>
{
    public DeleteShiftCommandValidator()
    {
        RuleFor(c => c.ShiftId).NotEmpty();
    }
}

public class DeleteShiftCommandHandler
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteShiftCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.PublicId == request.ShiftId, cancellationToken);
        Guard.Against.NotFound(request.ShiftId, shift);

        await _campusAccess.EnsureCampusAsync(shift.CampusId, ModuleCode.AcademicSetup, AccessRight.Delete, cancellationToken);
        shift.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ShiftChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Shift),
            targetId: shift.Id.ToString(CultureInfo.InvariantCulture));
    }
}
