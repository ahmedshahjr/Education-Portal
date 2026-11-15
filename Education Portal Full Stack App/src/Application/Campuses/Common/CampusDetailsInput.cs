using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Application.Campuses.Common;

/// <summary>
/// A campus's contact and registration details as the client sends them. The state, city and principal are public ids;
/// <see cref="CampusDetailsResolver"/> turns them into the internal keys before anything is saved.
/// Required: the address, the state, the city, the postal code, one to five phone numbers, the school number and the short
/// name. Optional: the website and the principal.
/// </summary>
public sealed record CampusDetailsInput
{
    public string Address { get; init; } = string.Empty;

    public Guid? StateId { get; init; }

    public Guid? CityId { get; init; }

    public string PostalCode { get; init; } = string.Empty;

    /// <summary>One to five phone numbers, in the order the school gives them.</summary>
    public IReadOnlyList<string> Phones { get; init; } = [];

    public string? Website { get; init; }

    public string SchoolNumber { get; init; } = string.Empty;

    public string ShortName { get; init; } = string.Empty;

    /// <summary>The principal: the public id of a person of the school whose user type is Principal.</summary>
    public Guid? PrincipalId { get; init; }
}

public class CampusDetailsInputValidator : AbstractValidator<CampusDetailsInput>
{
    public CampusDetailsInputValidator()
    {
        RuleFor(d => d.Address)
            .NotEmpty()
            .MaximumLength(CampusDetails.AddressMaxLength);

        RuleFor(d => d.StateId)
            .NotNull()
            .NotEqual(Guid.Empty);

        RuleFor(d => d.CityId)
            .NotNull()
            .NotEqual(Guid.Empty);

        RuleFor(d => d.PostalCode)
            .NotEmpty()
            .MaximumLength(ContactRules.PostalCodeMaxLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidPostalCode(value.Trim()))
            .WithMessage(_ => ValidationMessages.PostalCodeInvalid);

        RuleFor(d => d.Phones)
            .NotNull()
            .Must(phones => phones.Count(p => !string.IsNullOrWhiteSpace(p)) >= 1)
            .WithMessage(_ => ValidationMessages.CampusPhonesRequired)
            .Must(phones => phones.Count(p => !string.IsNullOrWhiteSpace(p)) <= Campus.MaxPhones)
            .WithMessage(_ => ValidationMessages.CampusPhonesTooMany);

        RuleForEach(d => d.Phones)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || CampusPhone.IsValid(phone))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);

        RuleFor(d => d.Website)
            .MaximumLength(ContactRules.WebsiteMaxLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidWebsite(value.Trim()))
            .WithMessage(_ => ValidationMessages.WebsiteInvalid);

        RuleFor(d => d.SchoolNumber)
            .NotEmpty()
            .MaximumLength(CampusDetails.SchoolNumberMaxLength);

        RuleFor(d => d.ShortName)
            .NotEmpty()
            .MaximumLength(CampusDetails.ShortNameMaxLength);
    }
}
