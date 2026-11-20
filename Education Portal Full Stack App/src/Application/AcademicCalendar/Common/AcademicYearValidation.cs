namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>Validation rules shared by the academic year commands.</summary>
public static class AcademicYearValidation
{
    public static void Dates<T>(AbstractValidator<T> validator, Func<T, DateOnly> start, Func<T, DateOnly> end)
    {
        validator.RuleFor(c => end(c))
            .Must((c, endDate) => endDate >= start(c))
            .WithName("EndDate")
            .OverridePropertyName("EndDate")
            .WithMessage(_ => ValidationMessages.DateRangeInvalid);

        validator.RuleFor(c => end(c))
            .Must((c, endDate) => endDate < start(c) || endDate.DayNumber - start(c).DayNumber < AcademicYear.MaxLengthInDays)
            .OverridePropertyName("EndDate")
            .WithMessage(_ => ValidationMessages.AcademicYearTooLong);
    }
}
