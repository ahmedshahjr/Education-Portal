using EducationPortal.Application.Modules.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>
/// The rule behind every module check (SEC-AUTHZ-01, -06): core always, paid only inside an active grant's period. A grant
/// on the school's main branch covers the whole school; a grant on another branch covers that branch only.
/// </summary>
public class ModuleEntitlementsTests
{
    private const int MainBranch = 1;
    private const int North = 5;
    private const int South = 6;

    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static ModuleEntitlements With(params ModuleGrant[] grants) =>
        new(["STUDENTS", "ATTENDANCE", "FEES"], ["STUDENTS"], grants);

    /// <summary>A grant on the main branch: the whole school.</summary>
    private static ModuleGrant WholeSchool(string code, DateTimeOffset from, DateTimeOffset? to) => new(code, MainBranch, true, from, to);

    [Test]
    public void ShouldAlwaysHaveACoreModule() =>
        With().Has("STUDENTS", null, Now).ShouldBeTrue();

    [Test]
    public void ShouldNotHaveAPaidModuleWithoutAGrantOrAnUnknownOne()
    {
        With().Has("FEES", null, Now).ShouldBeFalse();
        With().Has("NOPE", null, Now).ShouldBeFalse();
        ModuleEntitlements.None.Has("STUDENTS", null, Now).ShouldBeFalse();
    }

    [Test]
    public void ShouldHaveAGrantedModuleBetweenItsDatesWithEndExclusive()
    {
        var entitlements = With(WholeSchool("FEES", Now.AddDays(-1), Now.AddDays(1)));

        entitlements.Has("FEES", null, Now).ShouldBeTrue();
        entitlements.Has("FEES", null, Now.AddDays(-1)).ShouldBeTrue();      // starts inclusive
        entitlements.Has("FEES", null, Now.AddDays(-1).AddTicks(-1)).ShouldBeFalse();
        entitlements.Has("FEES", null, Now.AddDays(1)).ShouldBeFalse();      // ends exclusive
    }

    [Test]
    public void ShouldHaveAModuleWithNoEndForever() =>
        With(WholeSchool("FEES", Now.AddDays(-1), null)).Has("FEES", null, Now.AddYears(50)).ShouldBeTrue();

    [Test]
    public void ShouldLimitABranchGrantToThatBranch()
    {
        var entitlements = With(new ModuleGrant("FEES", North, false, Now.AddDays(-1), null));

        entitlements.Has("FEES", North, Now).ShouldBeTrue();
        entitlements.Has("FEES", South, Now).ShouldBeFalse();
        entitlements.Has("FEES", null, Now).ShouldBeFalse(); // no branch asked: a branch-only grant is not on (fail closed)
    }

    [Test]
    public void ShouldGiveAWholeSchoolGrantToEveryBranch()
    {
        var entitlements = With(WholeSchool("FEES", Now.AddDays(-1), null));

        entitlements.Has("FEES", North, Now).ShouldBeTrue();
        entitlements.Has("FEES", South, Now).ShouldBeTrue();
        entitlements.Has("FEES", null, Now).ShouldBeTrue();
    }

    [Test]
    public void ShouldTurnOffAnExpiredBranchGrantButKeepTheOtherBranchsGrant()
    {
        var entitlements = With(
            new ModuleGrant("FEES", North, false, Now.AddDays(-30), Now.AddDays(-1)),
            new ModuleGrant("FEES", South, false, Now.AddDays(-30), Now.AddDays(30)));

        entitlements.Has("FEES", North, Now).ShouldBeFalse();
        entitlements.Has("FEES", South, Now).ShouldBeTrue();
        entitlements.EnabledCodes(Now).ShouldNotContain("FEES");
    }

    [Test]
    public void ShouldTurnOffAnExpiredWholeSchoolGrantForEveryBranch()
    {
        var entitlements = With(WholeSchool("FEES", Now.AddDays(-30), Now.AddDays(-1)));

        entitlements.Has("FEES", null, Now).ShouldBeFalse();
        entitlements.Has("FEES", North, Now).ShouldBeFalse();
        entitlements.EnabledCodes(Now).ShouldNotContain("FEES");
    }

    [Test]
    public void ShouldNotLetAGrantForOneModuleOpenAnother() =>
        With(WholeSchool("FEES", Now.AddDays(-1), null)).Has("ATTENDANCE", null, Now).ShouldBeFalse();

    [Test]
    public void ShouldListOnlyTheWholeSchoolModulesInCatalogueOrder()
    {
        var entitlements = With(
            WholeSchool("FEES", Now.AddDays(-1), null),
            new ModuleGrant("ATTENDANCE", North, false, Now.AddDays(-1), null),          // one branch only: not whole-school
            WholeSchool("ATTENDANCE", Now.AddDays(-9), Now.AddDays(-1)));              // ended

        entitlements.EnabledCodes(Now).ShouldBe(["STUDENTS", "FEES"]);
    }
}
