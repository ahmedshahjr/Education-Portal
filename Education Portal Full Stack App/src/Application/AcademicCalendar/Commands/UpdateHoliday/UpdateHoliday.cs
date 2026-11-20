using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.UpdateHoliday;

/// <summary>Changes a holiday on the school calendar. Needs the Academic setup module. Audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record UpdateHolidayCommand : IRequest, IHolidayInput
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The public id, taken from the route.</summary>
    public Guid HolidayId { get; init; }

    public Guid? CampusId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public HolidayType Type { get; init; }

    public HolidayAudience AppliesTo { get; init; }

    public bool IsTentative { get; init; }
}

public class UpdateHolidayCommandValidator : AbstractValidator<UpdateHolidayCommand>
{
    public UpdateHolidayCommandValidator()
    {
        RuleFor(c => c.HolidayId).NotEmpty();
        Include(new HolidayInputValidator());
    }
}

public class UpdateHolidayCommandHandler : IRequestHandler<UpdateHolidayCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateHolidayCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var item = await _context.Holidays.FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.PublicId == request.HolidayId, cancellationToken);
        Guard.Against.NotFound(request.HolidayId, item);

        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);
        item.Change(campusId, request.Title, request.StartDate, request.EndDate, request.Type, request.AppliesTo, request.IsTentative);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Holiday),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));
    }
}
