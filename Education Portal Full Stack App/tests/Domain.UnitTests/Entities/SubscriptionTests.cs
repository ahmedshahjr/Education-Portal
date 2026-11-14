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

        var tenantModule = subscription.AddModule(Module("FEES", 7), TestBranches.MainBranch());

        tenantModule.ShouldSatisfyAllConditions(
            m => m.SchoolId.ShouldBe(9),
            m => m.ModuleId.ShouldBe(7),
            m => m.BranchId.ShouldBe(1),
            m => m.IsSchoolWide.ShouldBeTrue(),
            m => m.Status.ShouldBe(TenantModuleStatus.Active),
            m => m.ValidFrom.ShouldBe(Start),
            m => m.ValidTo.ShouldBe(subscription.EndDate));
        subscription.Modules.Count.ShouldBe(1);
        Should.Throw<InvalidOperationException>(() => subscription.AddModule(Module("FEES", 7), TestBranches.MainBranch()));
    }

    [Test]
    public void ShouldLimitAModuleGivenToAnotherBranchToThatBranch()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, 0);
        var north = Branch.Create(9, "North", "0002");
        north.Id = 2;

        var tenantModule = subscription.AddModule(Module("FEES", 7), north);

        tenantModule.ShouldSatisfyAllConditions(m => m.BranchId.ShouldBe(2), m => m.IsSchoolWide.ShouldBeFalse());
    }

    [Test]
    public void ShouldRefuseABranchOfAnotherSchool()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, 0);
        var other = Branch.Create(4, "Other", "0003");
        other.Id = 3;

        Should.Throw<ArgumentException>(() => subscription.AddModule(Module("FEES", 7), other));
    }

    [Test]
    public void ShouldRefuseAnUnsavedModule()
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 1m, Start, 0);

        Should.Throw<ArgumentOutOfRangeException>(() => subscription.AddModule(Module("FEES", 0), TestBranches.MainBranch()));
    }

    [Test]
    public void ShouldCreateABranchForASchool()
    {
        var branch = Branch.Create(9, " North campus ", "0004");

        branch.SchoolId.ShouldBe(9);
        branch.Name.ShouldBe("North campus");
        branch.Code.ShouldBe("0004");
        Should.Throw<ArgumentOutOfRangeException>(() => Branch.Create(0, "x", "0005"));
        Should.Throw<ArgumentException>(() => Branch.Create(9, " ", "0005"));
        Should.Throw<ArgumentOutOfRangeException>(() => Branch.Create(9, new string('a', Branch.NameMaxLength + 1), "0005"));
        Branch.Create(9, new string('a', Branch.NameMaxLength), "0005").Name.Length.ShouldBe(Branch.NameMaxLength);
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

    private static Subscription WithModules(params PortalModule[] modules)
    {
        var subscription = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 100m, Start, 0);
        foreach (var module in modules)
        {
            subscription.AddModule(module, TestBranches.MainBranch());
        }

        return subscription;
    }

    [Test]
    public void ShouldSuspendAndReactivateTheSubscriptionAndItsModules()
    {
        var subscription = WithModules(Module("FEES", 7), Module("EXAMS", 8));

        subscription.Suspend();

        subscription.Status.ShouldBe(SubscriptionStatus.Suspended);
        subscription.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Suspended);

        subscription.Reactivate();

        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active);
    }

    [Test]
    public void ShouldRefuseToSuspendTwiceOrAReactivationOfARunningSubscription()
    {
        var subscription = WithModules(Module("FEES", 7));

        Should.Throw<InvalidOperationException>(() => subscription.Reactivate());
        subscription.Suspend();
        Should.Throw<InvalidOperationException>(() => subscription.Suspend());
    }

    [Test]
    public void ShouldExtendThePeriodAndEveryModuleAndTheTrialWhileOnTrial()
    {
        var trial = Subscription.Create(9, null, Book(), BillingCycle.Monthly, 100m, Start, 14);
        trial.AddModule(Module("FEES", 7), TestBranches.MainBranch());

        trial.Extend(10);

        trial.ShouldSatisfyAllConditions(
            s => s.EndDate.ShouldBe(Start.AddDays(24)),
            s => s.TrialEndDate.ShouldBe(Start.AddDays(24)),
            s => s.Modules.Single().ValidTo.ShouldBe(Start.AddDays(24)));

        var active = WithModules(Module("FEES", 7));
        var end = active.EndDate;
        active.Extend(Subscription.MaxExtensionDays);
        active.EndDate.ShouldBe(end.AddDays(Subscription.MaxExtensionDays));
        active.TrialEndDate.ShouldBeNull();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(Subscription.MaxExtensionDays + 1)]
    public void ShouldRefuseABadExtension(int days) =>
        Should.Throw<ArgumentOutOfRangeException>(() => WithModules().Extend(days));

    [Test]
    public void ShouldEndModulesThatLeaveAndStartModulesThatJoinWhenReconfigured()
    {
        var fees = Module("FEES", 7);
        var exams = Module("EXAMS", 8);
        var library = Module("LIBRARY", 9);
        var subscription = WithModules(fees, exams);
        var now = Start.AddDays(5);

        subscription.Reconfigure(null, Book(), 250m, [fees, library], now, TestBranches.MainBranch());

        subscription.Amount.ShouldBe(250m);
        var byModule = subscription.Modules.ToDictionary(m => m.ModuleId);
        byModule[8].Status.ShouldBe(TenantModuleStatus.Expired);
        byModule[8].ValidTo.ShouldBe(now);
        byModule[7].Status.ShouldBe(TenantModuleStatus.Active);
        byModule[9].ShouldSatisfyAllConditions(
            m => m.Status.ShouldBe(TenantModuleStatus.Active),
            m => m.ValidFrom.ShouldBe(now),
            m => m.ValidTo.ShouldBe(subscription.EndDate));

        // A module that returns is restored for the rest of the period; no second row is made for it.
        subscription.Reconfigure(null, Book(), 300m, [fees, exams, library], now.AddDays(1), TestBranches.MainBranch());

        subscription.Modules.Count.ShouldBe(3);
        byModule[8].Status.ShouldBe(TenantModuleStatus.Active);
        byModule[8].ValidTo.ShouldBe(subscription.EndDate);
    }

    [Test]
    public void ShouldRefuseAReconfigurationInAnotherCurrencyOrWithABadAmount()
    {
        var subscription = WithModules(Module("FEES", 7));
        var dollars = Saved(PriceBook.Create("US", "USD"), 4);

        Should.Throw<ArgumentException>(() => subscription.Reconfigure(null, dollars, 1m, [], Start, TestBranches.MainBranch()));
        Should.Throw<ArgumentOutOfRangeException>(() => subscription.Reconfigure(null, Book(), 1.234m, [], Start, TestBranches.MainBranch()));
    }
}
