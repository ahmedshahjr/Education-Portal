using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Web.Services;

/// <summary>
/// Reads the caller's time zone from the <see cref="HeaderName"/> header, which the Angular app sets from the browser
/// (<c>Intl.DateTimeFormat().resolvedOptions().timeZone</c>). A missing or unknown value falls back to the school's
/// country, then UTC. The value only changes how dates are shown, never what data is returned, so it needs no further
/// trust (SEC-INPUT-01: only IANA ids the runtime knows are accepted).
/// </summary>
public sealed class RequestTimeZone(IHttpContextAccessor httpContextAccessor, ITenantLocaleSettings settings) : IRequestTimeZone
{
    public const string HeaderName = "X-Time-Zone";

    private TimeZoneInfo? _zone;

    public async Task<TimeZoneInfo> GetAsync(CancellationToken cancellationToken)
    {
        if (_zone is not null)
        {
            return _zone;
        }

        var requested = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();

        _zone = Country.IsValidTimeZone(requested)
            ? TimeZoneInfo.FindSystemTimeZoneById(requested!)
            : FindOrUtc((await settings.GetAsync(cancellationToken)).TimeZoneId);

        return _zone;
    }

    private static TimeZoneInfo FindOrUtc(string timeZoneId) =>
        TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var zone) ? zone : TimeZoneInfo.Utc;
}
