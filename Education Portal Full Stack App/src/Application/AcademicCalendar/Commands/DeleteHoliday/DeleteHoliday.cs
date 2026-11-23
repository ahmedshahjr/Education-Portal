using EducationPortal.Domain.Enums;
using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.DeleteHoliday;

/// <summary>
/// Deletes a holiday from the school calendar. The row is kept (soft delete) and hidden everywhere. Needs the Academic setup
/// module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Delete)]
public record DeleteHolidayCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The public id, taken from the route.</summary>
    public Guid HolidayId { get; init; }
}

public class DeleteHolidayCommandValidator : AbstractValidator<DeleteHolidayCommand>
{
    public DeleteHolidayCommandValidator()
    {
        RuleFor(c => c.HolidayId).NotEmpty();
    }
}

public class DeleteHolidayCommandHandler
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteHolidayCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var item = await _context.Holidays.FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.PublicId == request.HolidayId, cancellationToken);
        Guard.Against.NotFound(request.HolidayId, item);

        if (item.CampusId is { } itemCampusToCheck)
        {
            await _campusAccess.EnsureCampusAsync(itemCampusToCheck, ModuleCode.AcademicSetup, AccessRight.Delete, cancellationToken);
        }
        item.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Holiday),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));
    }
}
