using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Domain.Communication;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Calendar.Queries.GetCalendar;

/// <summary>
/// The school events that overlap a range, earliest first. A school administrator sees them all; anyone else sees the events
/// that reach them. Needs the School module (a core module); no right is needed to read the calendar.
/// </summary>
[Authorize(Module = ModuleCode.School)]
public record GetCalendarQuery
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}

public class GetCalendarQueryValidator : AbstractValidator<GetCalendarQuery>
{
    /// <summary>The longest range a calendar view may ask for.</summary>
    public const int MaximumDays = 366;

    public GetCalendarQueryValidator()
    {
        RuleFor(q => q.To).GreaterThan(q => q.From).WithMessage(_ => ValidationMessages.CalendarRangeInvalid);
        RuleFor(q => q.To).LessThanOrEqualTo(q => q.From.AddDays(MaximumDays)).WithMessage(_ => ValidationMessages.CalendarRangeTooLong);
    }
}

public class GetCalendarQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetCalendarQueryHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<IReadOnlyList<SchoolEventDto>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.SchoolEvents.AsNoTracking()
            .Where(e => e.StartsAt < request.To && e.EndsAt > request.From)
            .OrderBy(e => e.StartsAt)
            .ToListAsync(cancellationToken);

        if (CourseOwnership.IsAdministrator(_user))
        {
            return events.Select(ToDto).ToList();
        }

        var type = await PersonTypeAsync(cancellationToken);

        return events
            .Where(e => AudienceRules.Visible(e.Audience, type))
            .Select(ToDto)
            .ToList();
    }

    private async Task<UserType?> PersonTypeAsync(CancellationToken cancellationToken)
    {
        var personId = await _identityService.FindPersonIdOfAccountAsync(_user.Id ?? string.Empty);
        if (personId is not { } person)
        {
            return null;
        }

        return await _context.UserDetails.AsNoTracking()
            .Where(u => u.Id == person)
            .Select(u => (UserType?)u.UserType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static SchoolEventDto ToDto(EducationPortal.Domain.Entities.SchoolEvent e) =>
        new(e.PublicId, e.Title, e.Description, e.StartsAt, e.EndsAt, e.Audience);
}
