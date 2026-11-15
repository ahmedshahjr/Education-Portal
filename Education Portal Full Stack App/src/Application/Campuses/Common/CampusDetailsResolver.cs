using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using FluentValidation.Results;

namespace EducationPortal.Application.Campuses.Common;

/// <summary>
/// Turns a campus's public ids into the internal keys and checks that each link belongs to the school: the state is in the
/// school's country, the city is in that state, and the principal is a principal of this school. Then the values are
/// checked by <see cref="CampusDetails.Create"/>. The state and city are required, so the validator has checked they are given.
/// </summary>
internal static class CampusDetailsResolver
{
    public static async Task<CampusDetails> ResolveAsync(
        IApplicationDbContext context,
        int schoolId,
        CampusDetailsInput input,
        CancellationToken cancellationToken)
    {
        var schoolCountryId = await context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => s.CountryId)
            .SingleAsync(cancellationToken);

        if (input.StateId is not { } stateGuid)
        {
            throw Refuse(nameof(CampusDetailsInput.StateId), ValidationMessages.CampusStateNotInCountry);
        }

        var state = await context.States
            .AsNoTracking()
            .Where(s => s.PublicId == stateGuid)
            .Select(s => new { s.Id, s.CountryId })
            .SingleOrDefaultAsync(cancellationToken);

        // A school with no country has no states to choose from.
        if (state is null || state.CountryId != schoolCountryId)
        {
            throw Refuse(nameof(CampusDetailsInput.StateId), ValidationMessages.CampusStateNotInCountry);
        }

        if (input.CityId is not { } cityGuid)
        {
            throw Refuse(nameof(CampusDetailsInput.CityId), ValidationMessages.CampusCityNotInState);
        }

        var city = await context.Cities
            .AsNoTracking()
            .Where(c => c.PublicId == cityGuid)
            .Select(c => new { c.Id, c.StateId })
            .SingleOrDefaultAsync(cancellationToken);

        if (city is null || city.StateId != state.Id)
        {
            throw Refuse(nameof(CampusDetailsInput.CityId), ValidationMessages.CampusCityNotInState);
        }

        int? principalId = null;
        if (input.PrincipalId is { } principalGuid)
        {
            var person = await context.UserDetails
                .AsNoTracking()
                .Where(u => u.PublicId == principalGuid)
                .Select(u => new { u.Id, u.SchoolId, u.UserType })
                .SingleOrDefaultAsync(cancellationToken);

            if (person is null || person.SchoolId != schoolId)
            {
                throw Refuse(nameof(CampusDetailsInput.PrincipalId), ValidationMessages.CampusPrincipalNotFound);
            }

            if (person.UserType != UserType.Principal)
            {
                throw Refuse(nameof(CampusDetailsInput.PrincipalId), ValidationMessages.CampusPrincipalNotPrincipal);
            }

            principalId = person.Id;
        }

        return CampusDetails.Create(
            input.Address,
            state.Id,
            city.Id,
            input.PostalCode,
            input.Phones,
            input.Website,
            input.SchoolNumber,
            input.ShortName,
            principalId);
    }

    private static EducationPortal.Application.Common.Exceptions.ValidationException Refuse(string property, string message) =>
        new([new ValidationFailure(property, message)]);
}
