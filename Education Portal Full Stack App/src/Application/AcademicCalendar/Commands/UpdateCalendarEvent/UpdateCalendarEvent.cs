using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.UpdateCalendarEvent;

/// <summary>Changes an event on the school calendar. Needs the Academic setup module. Audited (SEC-LOG-03).</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Edit)]
public record UpdateCalendarEventCommand : IRequest, ICalendarEventInput
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The public id, taken from the route.</summary>
    public Guid EventId { get; init; }

    public Guid? CampusId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateOnly EventDate { get; init; }

    /// <summary>The start in the school's local time, or <c>null</c> for an all-day event.</summary>
    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public CalendarEventType Type { get; init; }

    public CalendarEventAudience Audience { get; init; }
}

public class UpdateCalendarEventCommandValidator : AbstractValidator<UpdateCalendarEventCommand>
{
    public UpdateCalendarEventCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        Include(new CalendarEventInputValidator());
    }
}

public class UpdateCalendarEventCommandHandler : IRequestHandler<UpdateCalendarEventCommand>
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateCalendarEventCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var item = await _context.CalendarEvents.FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.PublicId == request.EventId, cancellationToken);
        Guard.Against.NotFound(request.EventId, item);

        await _campusAccess.EnsureCampusAsync(item.CampusId, cancellationToken);
        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);
        await _campusAccess.EnsureCampusAsync(campusId, cancellationToken);
        item.Change(campusId, request.Title, request.Description, request.EventDate, request.StartTime, request.EndTime, request.Type, request.Audience);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(CalendarEvent),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));
    }
}
