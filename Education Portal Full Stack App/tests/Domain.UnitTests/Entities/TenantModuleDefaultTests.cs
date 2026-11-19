using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A default grant: a module a school gets when it registers, free of charge, for the whole school and with no end.</summary>
public class TenantModuleDefaultTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static PortalModule Module(ModuleCode code, int id)
    {
        var module = PortalModule.Create(code, code.ToString(), "", isCore: false, sortOrder: 0);
        typeof(PortalModule).GetProperty(nameof(PortalModule.Id))!.SetValue(module, id);
        return module;
    }

    [Test]
    public void ShouldGiveTheWholeSchoolFromNowWithNoEndAndNoSubscription()
    {
        var grant = TenantModule.CreateDefault(TestCampuses.SchoolId, Module(ModuleCode.Students, 7), TestCampuses.MainCampus(), Now);

        grant.ShouldSatisfyAllConditions(
            g => g.SchoolId.ShouldBe(TestCampuses.SchoolId),
            g => g.ModuleId.ShouldBe(7),
            g => g.CampusId.ShouldBe(1),
            g => g.IsSchoolWide.ShouldBeTrue(),
            g => g.Status.ShouldBe(TenantModuleStatus.Active),
            g => g.ValidFrom.ShouldBe(Now),
            g => g.ValidTo.ShouldBeNull(),
            g => g.SubscriptionId.ShouldBeNull());
    }

    [Test]
    public void ShouldRefuseAGrantOnACampusThatIsNotTheMainOne()
    {
        var other = TestCampuses.MainCampus();
        typeof(Campus).GetProperty(nameof(Campus.Kind))!.SetValue(other, CampusKind.Other);

        Should.Throw<ArgumentException>(() => TenantModule.CreateDefault(TestCampuses.SchoolId, Module(ModuleCode.Students, 7), other, Now));
    }

    [Test]
    public void ShouldRefuseAGrantForAnotherSchoolsCampus()
    {
        Should.Throw<ArgumentException>(() => TenantModule.CreateDefault(TestCampuses.SchoolId + 1, Module(ModuleCode.Students, 7), TestCampuses.MainCampus(), Now));
    }

    [Test]
    public void ShouldRefuseAModuleThatIsNotSaved()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => TenantModule.CreateDefault(TestCampuses.SchoolId, PortalModule.Create(ModuleCode.Fees, "Fees", "", false, 0), TestCampuses.MainCampus(), Now));
    }
}
