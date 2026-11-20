namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>Validation rules shared by the term commands.</summary>
public static class TermValidation
{
    public static void Fields<T>(AbstractValidator<T> validator, Func<T, string> name, Func<T, int> sequence, Func<T, DateOnly> start, Func<T, DateOnly> end)
    {
        validator.RuleFor(c => name(c)).NotEmpty().MaximumLength(Term.NameMaxLength).OverridePropertyName("Name");
        validator.RuleFor(c => sequence(c)).InclusiveBetween(1, Term.MaxSequence).OverridePropertyName("Sequence");
        validator.RuleFor(c => end(c))
            .Must((c, endDate) => endDate >= start(c))
            .OverridePropertyName("EndDate")
            .WithMessage(_ => ValidationMessages.DateRangeInvalid);
    }
}
