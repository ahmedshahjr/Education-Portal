using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school's grading scale: a name and its bands (the table <c>GradingScales</c>). The bands are checked by
/// <see cref="GradeScale.Validate"/> before the scale is created (ADR-019).
/// </summary>
public class GradingScale : BaseAuditableEntity, ITenantOwned
{
    public const int NameMaxLength = 100;

    private readonly List<GradingBand> _bands = [];

    private GradingScale()
    {
        // For EF Core.
    }

    private GradingScale(int schoolId, string name)
    {
        SchoolId = schoolId;
        Name = name;
    }

    public static GradingScale Create(int schoolId, string name, IReadOnlyList<GradeBand> bands)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        GradeScale.Validate(bands);

        var scale = new GradingScale(schoolId, name.Trim());
        foreach (var band in bands)
        {
            scale._bands.Add(GradingBand.Create(schoolId, band));
        }

        return scale;
    }

    public int SchoolId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<GradingBand> Bands => _bands.AsReadOnly();
}
