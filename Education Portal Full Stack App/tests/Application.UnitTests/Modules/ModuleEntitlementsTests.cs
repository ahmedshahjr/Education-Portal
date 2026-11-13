using EducationPortal.Application.Modules.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>The rule behind every module check (SEC-AUTHZ-01, -06): core always, paid only inside an active grant's period.</summary>
public class ModuleEntitlementsTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static ModuleEntitlements With(params ModuleGrant[] grants) =>
        new(["STUDENTS", "ATTENDANCE", "FEES"], ["STUDENTS"], grants);

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
        var entitlements = With(new ModuleGrant("FEES", null, Now.AddDays(-1), Now.AddDays(1)));

        entitlements.Has("FEES", null, Now).ShouldBeTrue();
        entitlements.Has("FEES", null, Now.AddDays(-1)).ShouldBeTrue();      // starts inclusive
        entitlements.Has("FEES", null, Now.AddDays(-1).AddTicks(-1)).ShouldBeFalse();
        entitlements.Has("FEES", null, Now.AddDays(1)).ShouldBeFalse();      // ends exclusive
    }

    [Test]
    public void ShouldHaveAModuleWithNoEndForever() =>
        With(new ModuleGrant("FEES", null, Now.AddDays(-1), null)).Has("FEES", null, Now.AddYears(50)).ShouldBeTrue();

    [Test]
    public void ShouldLimitABranchGrantToThatBranch()
    {
        var entitlements = With(new ModuleGrant("FEES", 5, Now.AddDays(-1), null));

        entitlements.Has("FEES", 5, Now).ShouldBeTrue();
        entitlements.Has("FEES", 6, Now).ShouldBeFalse();
        entitlements.Has("FEES", null, Now).ShouldBeFalse();
    }

    [Test]
    public void ShouldListTheWholeSchoolModulesInCatalogueOrder()
    {
        var entitlements = With(
            new ModuleGrant("FEES", null, Now.AddDays(-1), null),
            new ModuleGrant("ATTENDANCE", 5, Now.AddDays(-1), null),     // one branch only: not whole-school
            new ModuleGrant("ATTENDANCE", null, Now.AddDays(-9), Now.AddDays(-1))); // ended

        entitlements.EnabledCodes(Now).ShouldBe(["STUDENTS", "FEES"]);
    }
}
