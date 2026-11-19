using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>One band of a grading scale: the percents it covers, its letter and grade points (the table <c>GradingBands</c>).</summary>
public class GradingBand : BaseAuditableEntity, ITenantOwned
{
    private GradingBand()
    {
        // For EF Core.
    }

    private GradingBand(int schoolId, GradeBand band)
    {
        SchoolId = schoolId;
        MinPercent = band.MinPercent;
        MaxPercent = band.MaxPercent;
        Letter = band.Letter.Trim();
        Points = band.Points;
    }

    public static GradingBand Create(int schoolId, GradeBand band)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        return new GradingBand(schoolId, band);
    }

    public int SchoolId { get; private set; }

    public int GradingScaleId { get; private set; }

    public decimal MinPercent { get; private set; }

    public decimal MaxPercent { get; private set; }

    public string Letter { get; private set; } = string.Empty;

    public decimal Points { get; private set; }
}
