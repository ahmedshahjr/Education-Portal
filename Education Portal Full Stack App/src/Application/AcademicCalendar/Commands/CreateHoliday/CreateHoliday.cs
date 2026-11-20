using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;

/// <summary>
/// Adds a holiday to the school calendar, for the whole school or one campus (CampusId). Needs the Academic setup module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Create)]
public record CreateHolidayCommand : IRequest<Guid>, IHolidayInput
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public Guid? CampusId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public HolidayType Type { get; init; }

    public HolidayAudience AppliesTo { get; init; }

    public bool IsTentative { get; init; }
}

public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        Include(new HolidayInputValidator());
    }
}

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, Guid>
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateHolidayCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);

        await _campusAccess.EnsureCampusAsync(campusId, cancellationToken);
        var item = Holiday.Create(schoolId, campusId, request.Title, request.StartDate, request.EndDate, request.Type, request.AppliesTo, request.IsTentative);
        _context.Holidays.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCalendarChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Holiday),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));

        return item.PublicId;
    }
}
