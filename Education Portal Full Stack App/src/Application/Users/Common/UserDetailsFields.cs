using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Application.Users.Common;

/// <summary>The fields of a person that the administrator sets when adding or changing them.</summary>
public abstract record UserDetailsFields
{
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Optional.</summary>
    public string? MiddleName { get; init; }

    public string LastName { get; init; } = string.Empty;

    public UserType UserType { get; init; }

    public string? Phone { get; init; }
}

/// <summary>The rules every person's fields follow (SEC-INPUT-01). Create and update use the same rules.</summary>
public abstract class UserDetailsFieldsValidator<T> : AbstractValidator<T>
    where T : UserDetailsFields
{
    protected UserDetailsFieldsValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.MiddleName)
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.UserType).IsInEnum();

        RuleFor(c => c.Phone)
            .MaximumLength(ContactRules.PhoneMaxLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidPhone(value.Trim()))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);
    }
}
