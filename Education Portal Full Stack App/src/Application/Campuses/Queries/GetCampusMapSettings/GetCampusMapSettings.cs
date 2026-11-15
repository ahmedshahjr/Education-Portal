using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Campuses.Queries.GetCampusMapSettings;

/// <summary>
/// The settings the campus map picker needs. <see cref="ApiKey"/> is the Google Maps JavaScript key, or <c>null</c> when none
/// is configured: the picker then takes the coordinates typed in. The key is a browser key, restricted by the site that uses
/// it, and is kept in the secret store, never in the repository (ADR-006).
/// </summary>
public sealed record CampusMapSettingsDto(string? ApiKey);

/// <summary>
/// Reads the map settings for the campus form. Needs the Campuses module, so only a school that may manage campuses gets the key.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Campus)]
public record GetCampusMapSettingsQuery : IRequest<CampusMapSettingsDto>;

public class GetCampusMapSettingsQueryHandler : IRequestHandler<GetCampusMapSettingsQuery, CampusMapSettingsDto>
{
    /// <summary>The secret that holds the Google Maps JavaScript API key.</summary>
    public static readonly SecretKey GoogleMapsApiKey = SecretKey.Platform("google-maps-api-key");

    private readonly ISecretProvider _secrets;

    public GetCampusMapSettingsQueryHandler(ISecretProvider secrets)
    {
        _secrets = secrets;
    }

    public async Task<CampusMapSettingsDto> Handle(GetCampusMapSettingsQuery request, CancellationToken cancellationToken)
    {
        var key = await _secrets.GetSecretAsync(GoogleMapsApiKey, cancellationToken);

        return new CampusMapSettingsDto(string.IsNullOrWhiteSpace(key) ? null : key.Trim());
    }
}
