using EducationPortal.Domain.Enums;

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

    /// <summary>The plans with their paid modules. A plan's own code stays data: the master can add plans, and a school can have its own.</summary>
    public static IReadOnlyList<(string Code, string Name, string Description, IReadOnlyList<ModuleCode> Modules)> Plans { get; } =
    [
        (Starter, "Starter", "Core modules with attendance and exams.",
            [ModuleCode.Attendance, ModuleCode.Exams]),
        (Standard, "Standard", "Starter plus admissions, fees, parent communication and timetable.",
            [ModuleCode.Attendance, ModuleCode.Exams, ModuleCode.Admissions, ModuleCode.Fees, ModuleCode.ParentCommunication, ModuleCode.Timetable]),
        (Full, "Full", "Every module.",
            [ModuleCode.Attendance, ModuleCode.Exams, ModuleCode.Admissions, ModuleCode.Fees, ModuleCode.ParentCommunication, ModuleCode.Timetable,
             ModuleCode.HrPayroll, ModuleCode.Transport, ModuleCode.Library, ModuleCode.Lms, ModuleCode.Hostel]),
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
