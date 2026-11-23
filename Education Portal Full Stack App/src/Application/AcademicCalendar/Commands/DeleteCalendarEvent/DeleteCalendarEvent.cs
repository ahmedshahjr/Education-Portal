using EducationPortal.Domain.Enums;
using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.DeleteCalendarEvent;

/// <summary>
/// Deletes an event from the school calendar. The row is kept (soft delete) and hidden everywhere. Needs the Academic setup
/// module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Delete)]
public record DeleteCalendarEventCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The public id, taken from the route.</summary>
    public Guid EventId { get; init; }
}

public class DeleteCalendarEventCommandValidator : AbstractValidator<DeleteCalendarEventCommand>
{
    public DeleteCalendarEventCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
    }
}

public class DeleteCalendarEventCommandHandler
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteCalendarEventCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var item = await _context.CalendarEvents.FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.PublicId == request.EventId, cancellationToken);
        Guard.Against.NotFound(request.EventId, item);

        if (item.CampusId is { } itemCampusToCheck)
        {
            await _campusAccess.EnsureCampusAsync(itemCampusToCheck, ModuleCode.AcademicSetup, AccessRight.Delete, cancellationToken);
        }
        item.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(CalendarEvent),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));
    }
}
