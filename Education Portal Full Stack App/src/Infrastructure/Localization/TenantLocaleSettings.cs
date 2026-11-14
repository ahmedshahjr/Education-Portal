using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Infrastructure.Localization;

/// <summary>
/// Reads the current school's country settings once per request (the request localization middleware and the
/// localization endpoint both ask). The school comes only from the host-resolved tenant (SEC-AUTHZ-04). Nothing is
/// cached across requests, so a change in the master portal applies to the next request.
/// </summary>
internal sealed class TenantLocaleSettings(ApplicationDbContext context, ICurrentTenant tenant) : ITenantLocaleSettings
{
    private TenantLocale? _locale;

    public async Task<TenantLocale> GetAsync(CancellationToken cancellationToken)
    {
        if (_locale is not null)
        {
            return _locale;
        }

        if (tenant.SchoolId is not { } schoolId)
        {
            return TenantLocale.Default;
        }

        var locale = await context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Join(context.Countries, s => s.CountryId, c => (int?)c.Id, (s, c) => new TenantLocale(c.Code, c.Languages, c.DefaultLanguage, c.TimeZoneId, c.CurrencyCode))
            .SingleOrDefaultAsync(cancellationToken);

        _locale = locale ?? TenantLocale.Default;

        return _locale;
    }
}
