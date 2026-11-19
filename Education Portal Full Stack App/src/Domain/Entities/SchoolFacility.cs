using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A facility a school lists on its public page (the table <c>SchoolFacilities</c>), with an optional note such as "Buses
/// cover 12 routes". A school lists each kind at most once.
/// </summary>
public class SchoolFacility : BaseAuditableEntity, ITenantOwned
{
    public const int DescriptionMaxLength = 300;

    private SchoolFacility()
    {
        // For EF Core.
    }

    public static SchoolFacility Create(int schoolId, FacilityKind kind, string? description)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        var facility = new SchoolFacility { SchoolId = schoolId, Kind = kind };
        facility.Describe(description);
        return facility;
    }

    public int SchoolId { get; private set; }

    public FacilityKind Kind { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Replaces the note. A blank note is stored as <c>null</c>.</summary>
    public void Describe(string? description)
    {
        var trimmed = description?.Trim();
        if (!string.IsNullOrEmpty(trimmed))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, DescriptionMaxLength, nameof(description));
        }

        Description = string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
