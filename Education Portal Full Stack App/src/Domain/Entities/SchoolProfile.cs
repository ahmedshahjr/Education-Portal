using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a school says about itself on its public page (the table <c>SchoolProfiles</c>): one per school. The page is
/// reached on the school's own domain, so a visitor never sees another school's profile. It stays a draft until the school
/// publishes it; only a published profile is shown to the public. Contact details, address and map place come from the
/// campuses, and the logo from the branding.
/// </summary>
public class SchoolProfile : BaseAuditableEntity, ITenantOwned
{
    public const int TaglineMaxLength = 200;
    public const int AboutMaxLength = 4000;
    public const int VisitingHoursMaxLength = 200;
    public const int MinimumEstablishedYear = 1800;

    private SchoolProfile()
    {
        // For EF Core.
        Tagline = string.Empty;
        About = string.Empty;
    }

    /// <summary>Creates the empty draft of a school's page.</summary>
    public static SchoolProfile Create(int schoolId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        return new SchoolProfile { SchoolId = schoolId, GenderPolicy = SchoolGenderPolicy.CoEducation };
    }

    public int SchoolId { get; private set; }

    public string Tagline { get; private set; }

    public string About { get; private set; }

    /// <summary>The year the school opened, when it wants to say.</summary>
    public int? EstablishedYear { get; private set; }

    public SchoolGenderPolicy GenderPolicy { get; private set; }

    /// <summary>For example "Monday to Friday, 8:00 to 14:00". Optional.</summary>
    public string? VisitingHours { get; private set; }

    /// <summary>A link to a virtual tour. Optional; an absolute http or https address.</summary>
    public string? VirtualTourUrl { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>Replaces the text of the page. The established year is not after <paramref name="today"/>'s year.</summary>
    public void Update(string tagline, string about, int? establishedYear, SchoolGenderPolicy genderPolicy, string? visitingHours, string? virtualTourUrl, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(tagline);
        ArgumentNullException.ThrowIfNull(about);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tagline.Trim().Length, TaglineMaxLength, nameof(tagline));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(about.Trim().Length, AboutMaxLength, nameof(about));

        if (establishedYear is { } year && (year < MinimumEstablishedYear || year > today.Year))
        {
            throw new ArgumentOutOfRangeException(nameof(establishedYear), establishedYear, "The established year is not possible.");
        }

        if (!Enum.IsDefined(genderPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(genderPolicy));
        }

        var hours = visitingHours?.Trim();
        if (!string.IsNullOrEmpty(hours))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(hours.Length, VisitingHoursMaxLength, nameof(visitingHours));
        }

        var tour = virtualTourUrl?.Trim();
        if (!string.IsNullOrEmpty(tour) && !ContactRules.IsValidWebsite(tour))
        {
            throw new ArgumentException("A virtual tour is an absolute http or https address.", nameof(virtualTourUrl));
        }

        Tagline = tagline.Trim();
        About = about.Trim();
        EstablishedYear = establishedYear;
        GenderPolicy = genderPolicy;
        VisitingHours = string.IsNullOrEmpty(hours) ? null : hours;
        VirtualTourUrl = string.IsNullOrEmpty(tour) ? null : tour;
    }

    /// <summary>Shows the page to the public. A page needs a tagline and an about text first.</summary>
    public void Publish(DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(Tagline) || string.IsNullOrWhiteSpace(About))
        {
            throw new InvalidOperationException("A page needs a tagline and an about text before it is published.");
        }

        if (IsPublished)
        {
            return;
        }

        IsPublished = true;
        PublishedAt = now;
    }

    /// <summary>Hides the page from the public. The text is kept.</summary>
    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
    }
}
