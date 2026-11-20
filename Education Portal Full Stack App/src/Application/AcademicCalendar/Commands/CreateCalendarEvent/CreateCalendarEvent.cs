using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.CreateCalendarEvent;

/// <summary>
/// Adds an event to the school calendar, for the whole school or one campus (CampusId). Needs the Academic setup module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record CreateCalendarEventCommand : IRequest<Guid>, ICalendarEventInput
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

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

public class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator()
    {
        Include(new CalendarEventInputValidator());
    }
}

public class CreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateCalendarEventCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);

        var item = CalendarEvent.Create(schoolId, campusId, request.Title, request.Description, request.EventDate, request.StartTime, request.EndTime, request.Type, request.Audience);
        _context.CalendarEvents.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(CalendarEvent),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));

        return item.PublicId;
    }
}
