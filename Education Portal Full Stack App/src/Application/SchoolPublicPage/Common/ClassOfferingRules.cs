namespace EducationPortal.Application.SchoolPublicPage.Common;

/// <summary>The formats an offering's academic year and currency must have (the same checks as the entity).</summary>
public static class ClassOfferingRules
{
    /// <summary>For example <c>2026</c> or <c>2026-27</c>.</summary>
    public const string AcademicYearPattern = @"^\d{4}(-\d{2})?$";

    /// <summary>A three-letter ISO 4217 code, for example <c>PKR</c>.</summary>
    public const string CurrencyPattern = "^[A-Za-z]{3}$";
}
