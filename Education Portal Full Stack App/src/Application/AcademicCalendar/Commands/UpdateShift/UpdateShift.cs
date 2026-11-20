using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.UpdateShift;

/// <summary>Changes a shift's name and times. Its campus does not change. Needs the Academic setup module. Audited (SEC-LOG-03).</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Edit)]
public record UpdateShiftCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The shift's public id, taken from the route.</summary>
    public Guid ShiftId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(c => c.ShiftId).NotEmpty();
        ShiftRules.Fields(this, c => c.Name, c => c.StartTime, c => c.EndTime);
    }
}

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand>
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateShiftCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.PublicId == request.ShiftId, cancellationToken);
        Guard.Against.NotFound(request.ShiftId, shift);

        await _campusAccess.EnsureCampusAsync(shift.CampusId, cancellationToken);
        await ShiftRules.EnsureNameFreeAsync(_context, shift.CampusId, shift.Id, request.Name, cancellationToken);

        shift.Change(request.Name, request.StartTime, request.EndTime);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ShiftChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Shift),
            targetId: shift.Id.ToString(CultureInfo.InvariantCulture));
    }
}
