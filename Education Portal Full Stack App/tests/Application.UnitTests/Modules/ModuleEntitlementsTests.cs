using EducationPortal.Domain.Enums;
using EducationPortal.Application.Modules.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>
/// The rule behind every module check (SEC-AUTHZ-01, -06): every module, core or paid, is on only inside an active grant's
/// period. A grant on the school's main campus covers the whole school; a grant on another campus covers that campus only.
/// </summary>
public class ModuleEntitlementsTests
{
    private const int MainCampus = 1;
    private const int North = 5;
    private const int South = 6;

    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static ModuleEntitlements With(params ModuleGrant[] grants) =>
        new([ModuleCode.Students, ModuleCode.Attendance, ModuleCode.Fees], grants);

    /// <summary>A grant on the main campus: the whole school.</summary>
    private static ModuleGrant WholeSchool(ModuleCode code, DateTimeOffset from, DateTimeOffset? to) => new(code, MainCampus, true, from, to);

    [Test]
    public void ShouldNotHaveACoreModuleWithoutAGrant_SoTheMasterCanSwitchItOff()
    {
        With().Has(ModuleCode.Students, null, Now).ShouldBeFalse();
        With(WholeSchool(ModuleCode.Students, Now.AddDays(-1), null)).Has(ModuleCode.Students, null, Now).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotHaveAPaidModuleWithoutAGrantOrAnUnknownOne()
    {
        With().Has(ModuleCode.Fees, null, Now).ShouldBeFalse();
        With().Has((ModuleCode)99, null, Now).ShouldBeFalse();
        ModuleEntitlements.None.Has(ModuleCode.Students, null, Now).ShouldBeFalse();
    }

    [Test]
    public void ShouldHaveAGrantedModuleBetweenItsDatesWithEndExclusive()
    {
        var entitlements = With(WholeSchool(ModuleCode.Fees, Now.AddDays(-1), Now.AddDays(1)));

        entitlements.Has(ModuleCode.Fees, null, Now).ShouldBeTrue();
        entitlements.Has(ModuleCode.Fees, null, Now.AddDays(-1)).ShouldBeTrue();      // starts inclusive
        entitlements.Has(ModuleCode.Fees, null, Now.AddDays(-1).AddTicks(-1)).ShouldBeFalse();
        entitlements.Has(ModuleCode.Fees, null, Now.AddDays(1)).ShouldBeFalse();      // ends exclusive
    }

    [Test]
    public void ShouldHaveAModuleWithNoEndForever() =>
        With(WholeSchool(ModuleCode.Fees, Now.AddDays(-1), null)).Has(ModuleCode.Fees, null, Now.AddYears(50)).ShouldBeTrue();

    [Test]
    public void ShouldLimitACampusGrantToThatCampus()
    {
        var entitlements = With(new ModuleGrant(ModuleCode.Fees, North, false, Now.AddDays(-1), null));

        entitlements.Has(ModuleCode.Fees, North, Now).ShouldBeTrue();
        entitlements.Has(ModuleCode.Fees, South, Now).ShouldBeFalse();
        entitlements.Has(ModuleCode.Fees, null, Now).ShouldBeFalse(); // no campus asked: a campus-only grant is not on (fail closed)
    }

    [Test]
    public void ShouldGiveAWholeSchoolGrantToEveryCampus()
    {
        var entitlements = With(WholeSchool(ModuleCode.Fees, Now.AddDays(-1), null));

        entitlements.Has(ModuleCode.Fees, North, Now).ShouldBeTrue();
        entitlements.Has(ModuleCode.Fees, South, Now).ShouldBeTrue();
        entitlements.Has(ModuleCode.Fees, null, Now).ShouldBeTrue();
    }

    [Test]
    public void ShouldTurnOffAnExpiredCampusGrantButKeepTheOtherCampussGrant()
    {
        var entitlements = With(
            new ModuleGrant(ModuleCode.Fees, North, false, Now.AddDays(-30), Now.AddDays(-1)),
            new ModuleGrant(ModuleCode.Fees, South, false, Now.AddDays(-30), Now.AddDays(30)));

        entitlements.Has(ModuleCode.Fees, North, Now).ShouldBeFalse();
        entitlements.Has(ModuleCode.Fees, South, Now).ShouldBeTrue();
        entitlements.EnabledCodes(Now).ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public void ShouldTurnOffAnExpiredWholeSchoolGrantForEveryCampus()
    {
        var entitlements = With(WholeSchool(ModuleCode.Fees, Now.AddDays(-30), Now.AddDays(-1)));

        entitlements.Has(ModuleCode.Fees, null, Now).ShouldBeFalse();
        entitlements.Has(ModuleCode.Fees, North, Now).ShouldBeFalse();
        entitlements.EnabledCodes(Now).ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public void ShouldNotLetAGrantForOneModuleOpenAnother() =>
        With(WholeSchool(ModuleCode.Fees, Now.AddDays(-1), null)).Has(ModuleCode.Attendance, null, Now).ShouldBeFalse();

    [Test]
    public void ShouldListOnlyTheWholeSchoolModulesInCatalogueOrder()
    {
        var entitlements = With(
            WholeSchool(ModuleCode.Students, Now.AddDays(-1), null),
            WholeSchool(ModuleCode.Fees, Now.AddDays(-1), null),
            new ModuleGrant(ModuleCode.Attendance, North, false, Now.AddDays(-1), null),          // one campus only: not whole-school
            WholeSchool(ModuleCode.Attendance, Now.AddDays(-9), Now.AddDays(-1)));              // ended

        entitlements.EnabledCodes(Now).ShouldBe([ModuleCode.Students, ModuleCode.Fees]);
    }
}
