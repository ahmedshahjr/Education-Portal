using EducationPortal.Domain.Enums;
using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.CreateShift;

/// <summary>
/// Adds a shift to a campus of the school, for example Morning from 07:30 to 13:00. Names are unique within a campus.
/// Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Create)]
public record CreateShiftCommand : IRequest<Guid>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The campus's public id. A campus of another school is not found.</summary>
    public Guid CampusId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(c => c.CampusId).NotEmpty();
        ShiftRules.Fields(this, c => c.Name, c => c.StartTime, c => c.EndTime);
    }
}

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Guid>
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateShiftCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var campusId = await _context.Campuses.Where(c => c.SchoolId == schoolId).ResolveIdAsync(request.CampusId, cancellationToken);

        await _campusAccess.EnsureCampusAsync(campusId, ModuleCode.AcademicSetup, AccessRight.Create, cancellationToken);
        await ShiftRules.EnsureNameFreeAsync(_context, campusId, null, request.Name, cancellationToken);

        var shift = Shift.Create(schoolId, campusId, request.Name, request.StartTime, request.EndTime);
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ShiftChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Shift),
            targetId: shift.Id.ToString(CultureInfo.InvariantCulture));

        return shift.PublicId;
    }
}
