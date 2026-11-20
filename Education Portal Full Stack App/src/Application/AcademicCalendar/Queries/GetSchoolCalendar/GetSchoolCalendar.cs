using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Queries.GetSchoolCalendar;

/// <summary>
/// The school calendar between two dates (both included): the holidays that touch the range and the events in it. With a
/// campus, only what is for that campus or for every campus. The range is at most about a year. Needs the Academic setup module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record GetSchoolCalendarQuery : IRequest<SchoolCalendarDto>
{
    /// <summary>The longest range one request can read.</summary>
    public const int MaxRangeInDays = 400;

    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    /// <summary>Only this campus and the whole school, or every campus when left out.</summary>
    public Guid? CampusId { get; init; }
}

public class GetSchoolCalendarQueryValidator : AbstractValidator<GetSchoolCalendarQuery>
{
    public GetSchoolCalendarQueryValidator()
    {
        RuleFor(q => q.To)
            .Must((q, to) => to >= q.From)
            .WithMessage(_ => ValidationMessages.DateRangeInvalid);
        RuleFor(q => q.To)
            .Must((q, to) => to < q.From || to.DayNumber - q.From.DayNumber < GetSchoolCalendarQuery.MaxRangeInDays);
        RuleFor(q => q.CampusId).NotEqual(Guid.Empty).When(q => q.CampusId.HasValue);
    }
}

public class GetSchoolCalendarQueryHandler : IRequestHandler<GetSchoolCalendarQuery, SchoolCalendarDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetSchoolCalendarQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<SchoolCalendarDto> Handle(GetSchoolCalendarQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);
        var (from, to) = (request.From, request.To);

        var holidays = await _context.Holidays
            .AsNoTracking()
            .Where(h => h.SchoolId == schoolId && h.StartDate <= to && h.EndDate >= from)
            .Where(h => campusId == null || h.CampusId == null || h.CampusId == campusId)
            .OrderBy(h => h.StartDate)
            .ThenBy(h => h.Id)
            .Select(h => new { h.PublicId, h.CampusId, h.Title, h.StartDate, h.EndDate, h.Type, h.AppliesTo, h.IsTentative })
            .ToListAsync(cancellationToken);

        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Where(e => e.SchoolId == schoolId && e.EventDate >= from && e.EventDate <= to)
            .Where(e => campusId == null || e.CampusId == null || e.CampusId == campusId)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime)
            .ThenBy(e => e.Id)
            .Select(e => new { e.PublicId, e.CampusId, e.Title, e.Description, e.EventDate, e.StartTime, e.EndTime, e.Type, e.Audience })
            .ToListAsync(cancellationToken);

        var campusIds = await _context.Campuses
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .ToDictionaryAsync(c => c.Id, c => c.PublicId, cancellationToken);
        Guid? Campus(int? id) => id is { } value && campusIds.TryGetValue(value, out var publicId) ? publicId : null;

        return new SchoolCalendarDto(
            holidays.Select(h => new HolidayDto(h.PublicId, Campus(h.CampusId), h.Title, h.StartDate, h.EndDate, h.Type, h.AppliesTo, h.IsTentative)).ToList(),
            events.Select(e => new CalendarEventDto(e.PublicId, Campus(e.CampusId), e.Title, e.Description, e.EventDate, e.StartTime, e.EndTime, e.Type, e.Audience)).ToList());
    }
}
