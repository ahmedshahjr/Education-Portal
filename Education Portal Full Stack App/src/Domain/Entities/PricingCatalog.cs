using EducationPortal.Domain.Constants;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The starting plans and price books, added by Development seeding when missing. Every price here is a placeholder
/// for the master tenant to replace in the master portal.
/// </summary>
public static class PricingCatalog
{
    public const string Starter = "STARTER";
    public const string Standard = "STANDARD";
    public const string Full = "FULL";

    /// <summary>The plans with the codes of their paid modules.</summary>
    public static IReadOnlyList<(string Code, string Name, string Description, IReadOnlyList<string> Modules)> Plans { get; } =
    [
        (Starter, "Starter", "Core modules with attendance and exams.",
            [ModuleCodes.Attendance, ModuleCodes.Exams]),
        (Standard, "Standard", "Starter plus admissions, fees, parent communication and timetable.",
            [ModuleCodes.Attendance, ModuleCodes.Exams, ModuleCodes.Admissions, ModuleCodes.Fees, ModuleCodes.ParentCommunication, ModuleCodes.Timetable]),
        (Full, "Full", "Every module.",
            [ModuleCodes.Attendance, ModuleCodes.Exams, ModuleCodes.Admissions, ModuleCodes.Fees, ModuleCodes.ParentCommunication, ModuleCodes.Timetable,
             ModuleCodes.HrPayroll, ModuleCodes.Transport, ModuleCodes.Library, ModuleCodes.Lms, ModuleCodes.Hostel]),
    ];

    /// <summary>
    /// The price books: country, currency, monthly price of each plan, and the monthly price of any one add-on module.
    /// Yearly prices are ten months' worth (two months free).
    /// </summary>
    public static IReadOnlyList<(string CountryCode, string CurrencyCode, IReadOnlyDictionary<string, decimal> MonthlyPlanPrices, decimal MonthlyModulePrice)> PriceBooks { get; } =
    [
        ("PK", "PKR", new Dictionary<string, decimal> { [Starter] = 25_000m, [Standard] = 65_000m, [Full] = 130_000m }, 8_000m),
        ("SA", "SAR", new Dictionary<string, decimal> { [Starter] = 375m, [Standard] = 925m, [Full] = 1_875m }, 110m),
        ("AE", "AED", new Dictionary<string, decimal> { [Starter] = 365m, [Standard] = 915m, [Full] = 1_835m }, 105m),
        ("US", "USD", new Dictionary<string, decimal> { [Starter] = 99m, [Standard] = 249m, [Full] = 499m }, 29m),
    ];

    public const int MonthsChargedPerYear = 10;
}
