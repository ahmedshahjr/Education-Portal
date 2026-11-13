using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SubscriptionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 31, 10, 0, 0, TimeSpan.Zero);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }

    private static PriceBook Book() => Saved(PriceBook.Create("PK", "PKR"), 3);

    private static PortalModule Module(string code, int id) => Saved(PortalModule.Create(code, code, "", isCore: false, sortOrder: 0), id);

    [Test]
    public void ShouldStartActiveForOnePeriodAndCopyTheCurrency()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 100m, Start, trialDays: 0);

        subscription.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Active),
            s => s.CurrencyCode.ShouldBe("PKR"),
            s => s.PriceBookId.ShouldBe(3),
            s => s.PlanId.ShouldBeNull(),
            s => s.TrialEndDate.ShouldBeNull(),
            s => s.EndDate.ShouldBe(new DateTimeOffset(2026, 2, 28, 10, 0, 0, TimeSpan.Zero)));
    }

    [Test]
    public void ShouldEndAYearlyPeriodAYearLater() =>
        Subscription.PeriodEnd(Start, BillingCycle.Yearly).ShouldBe(new DateTimeOffset(2027, 1, 31, 10, 0, 0, TimeSpan.Zero));

    [Test]
    public void ShouldStartATrialThatEndsOnTheTrialEnd()
    {
        var subscription = Subscription.Create(9, Saved(Plan.Create("A", "A", ""), 4), Book(), BillingCycle.Yearly, 0m, Start, trialDays: 14);

        subscription.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Trial),
            s => s.PlanId.ShouldBe(4),
            s => s.TrialEndDate.ShouldBe(Start.AddDays(14)),
            s => s.EndDate.ShouldBe(Start.AddDays(14)));
    }

    [TestCase(0, 1, 0, -1)]
    [TestCase(1, 0, 0, -1)]
    public void ShouldRefuseUnsavedSchoolOrBook(int schoolId, int bookId, int trial, int _)
    {
        var book = Saved(PriceBook.Create("PK", "PKR"), bookId);

        Should.Throw<ArgumentOutOfRangeException>(() => Subscription.Create(schoolId, null, book, BillingCycle.Monthly, 1m, Start, trial));
    }

    [TestCase(-1)]
    [TestCase(Subscription.MaxTrialDays + 1)]
    public void ShouldRefuseABadTrialLength(int days) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, days));

    [Test]
    public void ShouldAcceptTheLongestTrial() =>
        Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, Subscription.MaxTrialDays).Status.ShouldBe(SubscriptionStatus.Trial);

    [TestCase(-0.01)]
    [TestCase(1.234)]
    public void ShouldRefuseABadAmount(double amount) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Subscription.Create(9, null, Book(), BillingCycle.Monthly, (decimal)amount, Start, 0));

    [Test]
    public void ShouldRefuseAnUnknownBillingCycle() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Subscription.Create(9, null, Book(), (BillingCycle)99, 1m, Start, 0));

    [Test]
    public void ShouldGiveEachModuleOnceForTheSubscriptionsPeriod()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, 0);

        var tenantModule = subscription.AddModule(Module("FEES", 7));

        tenantModule.ShouldSatisfyAllConditions(
            m => m.SchoolId.ShouldBe(9),
            m => m.ModuleId.ShouldBe(7),
            m => m.BranchId.ShouldBeNull(),
            m => m.Status.ShouldBe(TenantModuleStatus.Active),
            m => m.ValidFrom.ShouldBe(Start),
            m => m.ValidTo.ShouldBe(subscription.EndDate));
        subscription.Modules.Count.ShouldBe(1);
        Should.Throw<InvalidOperationException>(() => subscription.AddModule(Module("FEES", 7)));
    }

    [Test]
    public void ShouldRefuseAnUnsavedModule()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, 0);

        Should.Throw<ArgumentOutOfRangeException>(() => subscription.AddModule(Module("FEES", 0)));
    }

    [Test]
    public void ShouldCreateABranchForASchool()
    {
        var branch = Branch.Create(9, " Main campus ");

        branch.SchoolId.ShouldBe(9);
        branch.Name.ShouldBe("Main campus");
        Should.Throw<ArgumentOutOfRangeException>(() => Branch.Create(0, "x"));
        Should.Throw<ArgumentException>(() => Branch.Create(9, " "));
        Should.Throw<ArgumentOutOfRangeException>(() => Branch.Create(9, new string('a', Branch.NameMaxLength + 1)));
        Branch.Create(9, new string('a', Branch.NameMaxLength)).Name.Length.ShouldBe(Branch.NameMaxLength);
    }

    [Test]
    public void ShouldAssignAndClearADataRegion()
    {
        var school = School.Create("School", "school1.com");

        school.AssignDataRegion(DataRegion.MiddleEast);
        school.DataRegion.ShouldBe(DataRegion.MiddleEast);
        school.AssignDataRegion(null);
        school.DataRegion.ShouldBeNull();
        Should.Throw<ArgumentOutOfRangeException>(() => school.AssignDataRegion((DataRegion)99));
    }
}
