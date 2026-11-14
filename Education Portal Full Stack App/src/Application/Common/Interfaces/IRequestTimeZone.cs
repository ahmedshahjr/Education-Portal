namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The time zone the caller reads dates and times in (ADR-005): their own, which the browser sends with every API
/// request, else the time zone of the school's country, else UTC. Dates are stored and sent over the API as UTC; use
/// this only where the server itself turns an instant into text or a calendar day (an export, an email, "today").
/// </summary>
public interface IRequestTimeZone
{
    Task<TimeZoneInfo> GetAsync(CancellationToken cancellationToken);
}
