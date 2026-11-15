using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A default grant: a module a school gets when it registers, free of charge, for the whole school and with no end.</summary>
public class TenantModuleDefaultTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static PortalModule Module(string code, int id)
    {
        var module = PortalModule.Create(code, code, "", isCore: false, sortOrder: 0);
        typeof(PortalModule).GetProperty(nameof(PortalModule.Id))!.SetValue(module, id);
        return module;
    }

    [Test]
    public void ShouldGiveTheWholeSchoolFromNowWithNoEndAndNoSubscription()
    {
        var grant = TenantModule.CreateDefault(TestBranches.SchoolId, Module("STUDENTS", 7), TestBranches.MainBranch(), Now);

        grant.ShouldSatisfyAllConditions(
            g => g.SchoolId.ShouldBe(TestBranches.SchoolId),
            g => g.ModuleId.ShouldBe(7),
            g => g.BranchId.ShouldBe(1),
            g => g.IsSchoolWide.ShouldBeTrue(),
            g => g.Status.ShouldBe(TenantModuleStatus.Active),
            g => g.ValidFrom.ShouldBe(Now),
            g => g.ValidTo.ShouldBeNull(),
            g => g.SubscriptionId.ShouldBeNull());
    }

    [Test]
    public void ShouldRefuseAGrantOnABranchThatIsNotTheMainOne()
    {
        var other = TestBranches.MainBranch();
        typeof(Branch).GetProperty(nameof(Branch.Kind))!.SetValue(other, BranchKind.Other);

        Should.Throw<ArgumentException>(() => TenantModule.CreateDefault(TestBranches.SchoolId, Module("STUDENTS", 7), other, Now));
    }

    [Test]
    public void ShouldRefuseAGrantForAnotherSchoolsBranch()
    {
        Should.Throw<ArgumentException>(() => TenantModule.CreateDefault(TestBranches.SchoolId + 1, Module("STUDENTS", 7), TestBranches.MainBranch(), Now));
    }

    [Test]
    public void ShouldRefuseAModuleThatIsNotSaved()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => TenantModule.CreateDefault(TestBranches.SchoolId, PortalModule.Create("FEES", "Fees", "", false, 0), TestBranches.MainBranch(), Now));
    }
}
