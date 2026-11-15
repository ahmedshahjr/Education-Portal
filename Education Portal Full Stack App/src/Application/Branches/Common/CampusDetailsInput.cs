using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Application.Branches.Common;

/// <summary>
/// A campus's contact and registration details as the client sends them. The state, city and principal are public ids;
/// <see cref="CampusDetailsResolver"/> turns them into the internal keys before anything is saved.
/// </summary>
public sealed record CampusDetailsInput
{
    public string? Address { get; init; }

    public Guid? StateId { get; init; }

    public Guid? CityId { get; init; }

    public string? PostalCode { get; init; }

    public string? Phone { get; init; }

    public string? Website { get; init; }

    public string? SchoolNumber { get; init; }

    public string? ShortName { get; init; }

    /// <summary>The principal: the public id of a person of the school whose user type is Principal.</summary>
    public Guid? PrincipalId { get; init; }
}

public class CampusDetailsInputValidator : AbstractValidator<CampusDetailsInput>
{
    public CampusDetailsInputValidator()
    {
        RuleFor(d => d.Address).MaximumLength(CampusDetails.AddressMaxLength);

        RuleFor(d => d.PostalCode)
            .MaximumLength(ContactRules.PostalCodeMaxLength)
            .Must(value => IsBlankOr(value, ContactRules.IsValidPostalCode))
            .WithMessage(_ => ValidationMessages.PostalCodeInvalid);

        RuleFor(d => d.Phone)
            .MaximumLength(ContactRules.PhoneMaxLength)
            .Must(value => IsBlankOr(value, ContactRules.IsValidPhone))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);

        RuleFor(d => d.Website)
            .MaximumLength(ContactRules.WebsiteMaxLength)
            .Must(value => IsBlankOr(value, ContactRules.IsValidWebsite))
            .WithMessage(_ => ValidationMessages.WebsiteInvalid);

        RuleFor(d => d.SchoolNumber).MaximumLength(CampusDetails.SchoolNumberMaxLength);
        RuleFor(d => d.ShortName).MaximumLength(CampusDetails.ShortNameMaxLength);

        // A city is chosen inside a state, so it cannot be given without one.
        RuleFor(d => d.CityId)
            .Must((details, cityId) => cityId is null || details.StateId is not null)
            .WithMessage(_ => ValidationMessages.CampusCityNeedsState);
    }

    /// <summary>A blank value is allowed (it means "not given"); a value that is not blank must pass the rule.</summary>
    private static bool IsBlankOr(string? value, Func<string, bool> isValid) =>
        string.IsNullOrWhiteSpace(value) || isValid(value.Trim());
}
