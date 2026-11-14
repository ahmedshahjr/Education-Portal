using NUnit.Framework;
using Shouldly;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.UnitTests.Entities;

public class PricingTests
{
    private static PortalModule Paid() => Saved(PortalModule.Create("FEES", "Fees", "", isCore: false, sortOrder: 1), 7);

    private static PortalModule Core() => Saved(PortalModule.Create("STUDENTS", "Students", "", isCore: true, sortOrder: 1), 1);

    private static PriceBook Book() => Saved(PriceBook.Create("PK", "PKR"), 3);

    private static Plan SavedPlan() => Saved(Plan.Create("A", "A", ""), 5);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }

    [TestCase("STARTER", true)]
    [TestCase("A1_B", true)]
    [TestCase("starter", false)]
    [TestCase("1A", false)]
    [TestCase("", false)]
    [TestCase("A B", false)]
    public void PlanCodeFormat(string code, bool valid) => Plan.IsValidCode(code).ShouldBe(valid);

    [Test]
    public void ShouldCreateAPlanActiveAndTrimmed()
    {
        var plan = Plan.Create("STARTER", " Starter ", " Basics. ");

        plan.ShouldSatisfyAllConditions(p => p.Code.ShouldBe("STARTER"), p => p.Name.ShouldBe("Starter"), p => p.Description.ShouldBe("Basics."), p => p.IsActive.ShouldBeTrue());
    }

    [Test]
    public void ShouldRefuseABadPlan()
    {
        Should.Throw<ArgumentException>(() => Plan.Create("bad code", "x", ""));
        Should.Throw<ArgumentException>(() => Plan.Create("OK", " ", ""));
    }

    [Test]
    public void ShouldNotPutACoreModuleInAPlan()
    {
        Should.Throw<ArgumentException>(() => PlanModule.Create(SavedPlan(), Core()));
    }

    [TestCase("PK", "PKR")]
    [TestCase("pk", "PKR")]
    public void ShouldCreateAPriceBookWithUpperCaseCountry(string country, string currency)
    {
        var book = PriceBook.Create(country, currency);

        book.CountryCode.ShouldBe("PK");
        book.CurrencyCode.ShouldBe(currency);
    }

    [TestCase("PAK", "PKR")]
    [TestCase("P", "PKR")]
    [TestCase("PK", "pkr")]
    [TestCase("PK", "PK")]
    [TestCase("PK", "")]
    public void ShouldRefuseABadPriceBook(string country, string currency) =>
        Should.Throw<ArgumentException>(() => PriceBook.Create(country, currency));

    [TestCase(0, true)]
    [TestCase(25000.5, true)]
    [TestCase(-0.01, false)]
    [TestCase(1.234, false)]
    public void PriceRules(double price, bool valid) => PriceBookItem.IsValidPrice((decimal)price).ShouldBe(valid);

    [Test]
    public void ShouldRefuseTheLargestPriceOverTheColumn() =>
        PriceBookItem.IsValidPrice(PriceBookItem.MaxPrice + 0.01m).ShouldBeFalse();

    [Test]
    public void ShouldPriceAPlanOrAPaidModuleButNotACoreModule()
    {
        var book = Book();

        PriceBookItem.ForModule(book, Paid(), BillingCycle.Monthly, 10m).ModuleId.ShouldBe(7);
        PriceBookItem.ForPlan(book, SavedPlan(), BillingCycle.Yearly, 0m).PlanId.ShouldBe(5);
        Should.Throw<ArgumentException>(() => PriceBookItem.ForModule(book, Core(), BillingCycle.Monthly, 10m));
        Should.Throw<ArgumentOutOfRangeException>(() => PriceBookItem.ForModule(book, Paid(), BillingCycle.Monthly, -1m));
    }

    [Test]
    public void ShouldSellOnlyPaidModulesAndGrowPlansInOrder()
    {
        var sizes = PricingCatalog.Plans.Select(p => p.Modules.Count).ToList();

        sizes.ShouldBe(sizes.OrderBy(n => n).ToList());
        PricingCatalog.Plans.SelectMany(p => p.Modules).Distinct().Intersect(ModuleCatalog.CreateModules().Where(m => m.IsCore).Select(m => m.Code)).ShouldBeEmpty();
        PricingCatalog.PriceBooks.Select(b => b.CountryCode).ShouldBe(["PK", "SA", "AE", "US"]);
    }
}
