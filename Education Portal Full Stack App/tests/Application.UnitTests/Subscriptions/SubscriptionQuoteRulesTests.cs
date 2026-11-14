using System.Globalization;
using System.Resources;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Subscriptions;

/// <summary>
/// Dependency resolution and price calculation for a subscription (ADR-011), without a database. Catalogue: core
/// STUDENTS and SCHOOL; paid ATTENDANCE, FEES (requires ATTENDANCE, which requires STUDENTS), EXAMS and LIBRARY.
/// </summary>
public class SubscriptionQuoteRulesTests
{
    private static readonly PriceBook Book = Saved(PriceBook.Create("PK", "PKR"), 1);

    private static readonly PortalModule School = Module("SCHOOL", 1, core: true);
    private static readonly PortalModule Students = Module("STUDENTS", 2, core: true);
    private static readonly PortalModule Attendance = Module("ATTENDANCE", 3);
    private static readonly PortalModule Fees = Module("FEES", 4);
    private static readonly PortalModule Exams = Module("EXAMS", 5);
    private static readonly PortalModule Library = Module("LIBRARY", 6);

    private static readonly PortalModule[] Catalogue = [School, Students, Attendance, Fees, Exams, Library];

    private static readonly (int, int)[] Dependencies = [(Fees.Id, Attendance.Id), (Attendance.Id, Students.Id)];

    private static readonly Plan Starter = Saved(Plan.Create("STARTER", "Starter", ""), 10);

    private static readonly PriceBookItem[] MonthlyPrices =
    [
        Saved(PriceBookItem.ForPlan(Book, Starter, BillingCycle.Monthly, 10000m), 1),
        Saved(PriceBookItem.ForModule(Book, Attendance, BillingCycle.Monthly, 1500.50m), 2),
        Saved(PriceBookItem.ForModule(Book, Fees, BillingCycle.Monthly, 2500m), 3),
        Saved(PriceBookItem.ForModule(Book, Exams, BillingCycle.Monthly, 3000m), 4),
    ];

    [Test]
    public void ShouldAlwaysIncludeEveryCoreModule()
    {
        var modules = Resolve(planModules: [], addOns: []);

        modules.ShouldBe([School, Students]);
    }

    [Test]
    public void ShouldAddWhatAnAddOnRequiresDirectlyAndIndirectly()
    {
        var modules = Resolve(planModules: [], addOns: ["FEES"]);

        modules.ShouldBe([School, Students, Attendance, Fees]);
    }

    [Test]
    public void ShouldCombineThePlansModulesWithTheAddOnsWithoutDuplicates()
    {
        var modules = Resolve(planModules: [Attendance.Id, Exams.Id], addOns: ["FEES", "EXAMS"]);

        modules.ShouldBe([School, Students, Attendance, Fees, Exams]);
    }

    [Test]
    public void ShouldRefuseAnUnknownAddOn() =>
        Refused(() => Resolve([], ["NOPE"])).ShouldBe(Message("UnknownModules"));

    [Test]
    public void ShouldRefuseSellingACoreModule() =>
        Refused(() => Resolve([], ["STUDENTS"])).ShouldBe(Message("CoreModuleNotSold"));

    [Test]
    public void ShouldRefuseAChoiceThatNeedsAnInactiveModule()
    {
        var attendance = Module("ATTENDANCE", 3);
        attendance.Deactivate();
        PortalModule[] catalogue = [School, Students, attendance, Fees];

        var error = Refused(() => SubscriptionQuoteRules.ResolveModules(catalogue, Dependencies, new HashSet<int>(), ["FEES"]));

        error.ShouldBe(Message("ModuleInactive"));
    }

    [Test]
    public void ShouldPriceAPlanAlone()
    {
        var modules = Resolve([Attendance.Id], []);

        SubscriptionQuoteRules.Price(Starter, new HashSet<int> { Attendance.Id }, modules, MonthlyPrices).ShouldBe(10000m);
    }

    [Test]
    public void ShouldAddTheAddOnsAndWhatTheyRequireButNotWhatThePlanAlreadyHas()
    {
        // The plan has ATTENDANCE; FEES needs it, so only FEES is added to the plan's price.
        var planModules = new HashSet<int> { Attendance.Id };
        var modules = Resolve([Attendance.Id], ["FEES"]);

        SubscriptionQuoteRules.Price(Starter, planModules, modules, MonthlyPrices).ShouldBe(12500m);
    }

    [Test]
    public void ShouldPriceAddOnsOnlyWithTheirRequiredPaidModulesAndFreeCoreModules()
    {
        var modules = Resolve([], ["FEES"]);

        // FEES + ATTENDANCE it requires; STUDENTS and SCHOOL are core and free. Exact decimal, no rounding.
        SubscriptionQuoteRules.Price(null, new HashSet<int>(), modules, MonthlyPrices).ShouldBe(4000.50m);
    }

    [Test]
    public void ShouldCostNothingForCoreModulesOnly() =>
        SubscriptionQuoteRules.Price(null, new HashSet<int>(), Resolve([], []), MonthlyPrices).ShouldBe(0m);

    [Test]
    public void ShouldRefuseAModuleWithoutAPrice() =>
        Refused(() => SubscriptionQuoteRules.Price(null, new HashSet<int>(), Resolve([], ["LIBRARY"]), MonthlyPrices))
            .ShouldBe(Message("PriceMissing"));

    [Test]
    public void ShouldRefuseAPlanWithoutAPrice()
    {
        var other = Saved(Plan.Create("FULL", "Full", ""), 11);

        Refused(() => SubscriptionQuoteRules.Price(other, new HashSet<int>(), Resolve([], []), MonthlyPrices))
            .ShouldBe(Message("PriceMissing"));
    }

    private static IReadOnlyList<PortalModule> Resolve(int[] planModules, string[] addOns) =>
        SubscriptionQuoteRules.ResolveModules(Catalogue, Dependencies, planModules.ToHashSet(), addOns);

    private static readonly ResourceManager Messages = new(
        "EducationPortal.Application.Common.Localization.ValidationMessages", typeof(SubscriptionQuoteRules).Assembly);

    private static string Message(string key) => Messages.GetString(key, CultureInfo.CurrentUICulture)!;

    private static string Refused(Action action) =>
        Should.Throw<ValidationException>(action).Errors.Values.SelectMany(m => m).ShouldHaveSingleItem();

    private static PortalModule Module(string code, int id, bool core = false) =>
        Saved(PortalModule.Create(code, code, "", core, id), id);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }
}
