using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// An event of the school calendar, with its audience (the table <c>SchoolEvents</c>). Times are stored in UTC. Creating it
/// creates one notification per recipient (ADR-019).
/// </summary>
public class SchoolEvent : BaseAuditableEntity, ITenantOwned
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private SchoolEvent()
    {
        // For EF Core.
    }

    private SchoolEvent(int schoolId, string title, string? description, DateTimeOffset startsAt, DateTimeOffset endsAt, EventAudience audience)
    {
        SchoolId = schoolId;
        Title = title;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Audience = audience;
    }

    public static SchoolEvent Create(int schoolId, string title, string? description, DateTimeOffset startsAt, DateTimeOffset endsAt, EventAudience audience)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var trimmedTitle = title.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedTitle.Length, TitleMaxLength, nameof(title));

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmedDescription is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedDescription.Length, DescriptionMaxLength, nameof(description));
        }

        if (endsAt <= startsAt)
        {
            throw new ArgumentException("An event ends after it starts.", nameof(endsAt));
        }

        if (!Enum.IsDefined(audience))
        {
            throw new ArgumentOutOfRangeException(nameof(audience), audience, "Not an audience.");
        }

        return new SchoolEvent(schoolId, trimmedTitle, trimmedDescription, startsAt.ToUniversalTime(), endsAt.ToUniversalTime(), audience);
    }

    public int SchoolId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public EventAudience Audience { get; private set; }
}
