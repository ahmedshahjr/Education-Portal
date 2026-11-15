using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using FluentValidation.Results;

namespace EducationPortal.Application.Branches.Common;

/// <summary>
/// Turns a campus's public ids into the internal keys and checks that each link belongs to the school: the state is in the
/// school's country, the city is in that state, and the principal is a principal of this school. Then the values are
/// checked by <see cref="CampusDetails.Create"/>.
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

        int? stateId = null;
        if (input.StateId is { } stateGuid)
        {
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

            stateId = state.Id;
        }

        int? cityId = null;
        if (input.CityId is { } cityGuid)
        {
            if (stateId is null)
            {
                throw Refuse(nameof(CampusDetailsInput.CityId), ValidationMessages.CampusCityNeedsState);
            }

            var city = await context.Cities
                .AsNoTracking()
                .Where(c => c.PublicId == cityGuid)
                .Select(c => new { c.Id, c.StateId })
                .SingleOrDefaultAsync(cancellationToken);

            if (city is null || city.StateId != stateId)
            {
                throw Refuse(nameof(CampusDetailsInput.CityId), ValidationMessages.CampusCityNotInState);
            }

            cityId = city.Id;
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
            stateId,
            cityId,
            input.PostalCode,
            input.Phone,
            input.Website,
            input.SchoolNumber,
            input.ShortName,
            principalId);
    }

    private static Application.Common.Exceptions.ValidationException Refuse(string property, string message) =>
        new([new ValidationFailure(property, message)]);
}
