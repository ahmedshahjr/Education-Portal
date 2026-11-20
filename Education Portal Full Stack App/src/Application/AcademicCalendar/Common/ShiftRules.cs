using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>Rules shared by the shift commands.</summary>
public static class ShiftRules
{
    public static void Fields<T>(AbstractValidator<T> validator, Func<T, string> name, Func<T, TimeOnly> start, Func<T, TimeOnly> end)
    {
        validator.RuleFor(c => name(c)).NotEmpty().MaximumLength(Shift.NameMaxLength).OverridePropertyName("Name");
        validator.RuleFor(c => end(c))
            .Must((c, endTime) => endTime > start(c))
            .OverridePropertyName("EndTime")
            .WithMessage(_ => ValidationMessages.TimeRangeInvalid);
    }

    /// <summary>Throws a validation error when the campus already has another shift with the name.</summary>
    public static async Task EnsureNameFreeAsync(IApplicationDbContext context, int campusId, int? exceptShiftId, string name, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        if (await context.Shifts.AnyAsync(s => s.CampusId == campusId && s.Id != exceptShiftId && s.Name == trimmed, cancellationToken))
        {
            throw CalendarRules.Invalid("Name", ValidationMessages.ShiftNameTaken);
        }
    }
}
