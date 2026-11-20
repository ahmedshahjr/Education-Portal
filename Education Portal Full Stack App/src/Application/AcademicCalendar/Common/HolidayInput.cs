namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>The fields of a holiday, shared by the create and update commands.</summary>
public interface IHolidayInput
{
    Guid? CampusId { get; }

    string Title { get; }

    DateOnly StartDate { get; }

    DateOnly EndDate { get; }

    HolidayType Type { get; }

    HolidayAudience AppliesTo { get; }

    bool IsTentative { get; }
}

public class HolidayInputValidator : AbstractValidator<IHolidayInput>
{
    public HolidayInputValidator()
    {
        RuleFor(c => c.CampusId).NotEqual(Guid.Empty).When(c => c.CampusId.HasValue);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Holiday.TitleMaxLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.AppliesTo).IsInEnum();
        RuleFor(c => c.EndDate)
            .Must((c, end) => end >= c.StartDate)
            .WithMessage(_ => ValidationMessages.DateRangeInvalid);
        RuleFor(c => c.EndDate)
            .Must((c, end) => end < c.StartDate || end.DayNumber - c.StartDate.DayNumber < Holiday.MaxLengthInDays)
            .WithMessage(_ => ValidationMessages.HolidayTooLong);
    }
}

/// <summary>The fields of a calendar event, shared by the create and update commands.</summary>
public interface ICalendarEventInput
{
    Guid? CampusId { get; }

    string Title { get; }

    string? Description { get; }

    DateOnly EventDate { get; }

    TimeOnly? StartTime { get; }

    TimeOnly? EndTime { get; }

    CalendarEventType Type { get; }

    CalendarEventAudience Audience { get; }
}

public class CalendarEventInputValidator : AbstractValidator<ICalendarEventInput>
{
    public CalendarEventInputValidator()
    {
        RuleFor(c => c.CampusId).NotEqual(Guid.Empty).When(c => c.CampusId.HasValue);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(CalendarEvent.TitleMaxLength);
        RuleFor(c => c.Description).MaximumLength(CalendarEvent.DescriptionMaxLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.Audience).IsInEnum();
        RuleFor(c => c.StartTime)
            .NotNull()
            .When(c => c.EndTime.HasValue)
            .WithMessage(_ => ValidationMessages.EndTimeNeedsStartTime);
        RuleFor(c => c.EndTime)
            .Must((c, end) => end > c.StartTime)
            .When(c => c.EndTime.HasValue && c.StartTime.HasValue)
            .WithMessage(_ => ValidationMessages.TimeRangeInvalid);
    }
}
