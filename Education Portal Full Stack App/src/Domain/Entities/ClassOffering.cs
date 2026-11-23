using System.Text.RegularExpressions;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A class a school offers for an academic year, with its fees and seats (the table <c>ClassOfferings</c>), shown on the
/// school's public page. It applies to the whole school or, when <see cref="CampusId"/> is set, to one campus. The fee is
/// shown only when <see cref="ShowFee"/> is on. A deleted offering stays in the table (soft delete).
/// </summary>
public partial class ClassOffering : BaseSoftDeletableEntity, ITenantOwned
{
    public const int AcademicYearMaxLength = 9;
    public const int CurrencyCodeLength = 3;

    private ClassOffering()
    {
        // For EF Core.
        AcademicYear = string.Empty;
        CurrencyCode = string.Empty;
    }

    /// <summary>
    /// Creates an offering. The academic year is like <c>2026</c> or <c>2026-27</c>. The fees are not negative, the currency
    /// is a three-letter code, and the free seats are not more than the total.
    /// </summary>
    public static ClassOffering Create(
        int schoolId,
        int campusId,
        ClassLevel level,
        string academicYear,
        decimal monthlyFee,
        decimal? admissionFee,
        string currencyCode,
        int seatsTotal,
        int seatsAvailable,
        AdmissionStatus status,
        bool showFee)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        if (!Enum.IsDefined(level))
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        var offering = new ClassOffering { SchoolId = schoolId, CampusId = campusId, Level = level };
        offering.Change(academicYear, monthlyFee, admissionFee, currencyCode, seatsTotal, seatsAvailable, status, showFee);
        return offering;
    }

    public int SchoolId { get; private set; }

    /// <summary>The campus this offering is for, or <c>null</c> for every campus of the school.</summary>
    public int CampusId { get; private set; }

    public ClassLevel Level { get; private set; }

    /// <summary>For example <c>2026-27</c>.</summary>
    public string AcademicYear { get; private set; }

    public decimal MonthlyFee { get; private set; }

    /// <summary>The one-time fee at admission, when there is one.</summary>
    public decimal? AdmissionFee { get; private set; }

    /// <summary>ISO 4217, for example <c>PKR</c>.</summary>
    public string CurrencyCode { get; private set; }

    public int SeatsTotal { get; private set; }

    public int SeatsAvailable { get; private set; }

    public AdmissionStatus Status { get; private set; }

    /// <summary>Whether the public page shows the fees.</summary>
    public bool ShowFee { get; private set; }

    /// <summary>Replaces everything but the school, campus and class, checked the same way as <see cref="Create"/>.</summary>
    public void Change(string academicYear, decimal monthlyFee, decimal? admissionFee, string currencyCode, int seatsTotal, int seatsAvailable, AdmissionStatus status, bool showFee)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted offering cannot be changed.");
        }

        ArgumentNullException.ThrowIfNull(academicYear);
        ArgumentNullException.ThrowIfNull(currencyCode);

        if (!AcademicYearPattern().IsMatch(academicYear.Trim()))
        {
            throw new ArgumentException("An academic year is like 2026 or 2026-27.", nameof(academicYear));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(monthlyFee);
        if (admissionFee is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(admissionFee));
        }

        var currency = currencyCode.Trim().ToUpperInvariant();
        if (!CurrencyPattern().IsMatch(currency))
        {
            throw new ArgumentException("A currency is a three-letter code such as PKR.", nameof(currencyCode));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(seatsTotal);
        ArgumentOutOfRangeException.ThrowIfNegative(seatsAvailable);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seatsAvailable, seatsTotal, nameof(seatsAvailable));

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        AcademicYear = academicYear.Trim();
        MonthlyFee = monthlyFee;
        AdmissionFee = admissionFee;
        CurrencyCode = currency;
        SeatsTotal = seatsTotal;
        SeatsAvailable = seatsAvailable;
        Status = status;
        ShowFee = showFee;
    }

    [GeneratedRegex(@"^\d{4}(-\d{2})?$")]
    private static partial Regex AcademicYearPattern();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyPattern();
}
