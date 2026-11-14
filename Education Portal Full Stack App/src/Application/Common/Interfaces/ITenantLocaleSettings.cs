using EducationPortal.Application.Localization.Common;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The regional settings of the school that owns the request's host: the languages it offers, the default language,
/// time zone and currency, from the country the master tenant assigned to it. The school comes only from the host
/// (SEC-AUTHZ-04).
/// </summary>
public interface ITenantLocaleSettings
{
    /// <summary>The current school's settings, or <see cref="TenantLocale.Default"/> when it has no country.</summary>
    Task<TenantLocale> GetAsync(CancellationToken cancellationToken);
}
