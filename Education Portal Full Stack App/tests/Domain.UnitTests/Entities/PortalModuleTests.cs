using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>
/// Every module can be switched off by the master, core ones included: an inactive module is off for every school. A
/// registering school gets the modules that are on by default. Core modules are part of every subscription.
/// </summary>
public class PortalModuleTests
{
    [Test]
    public void ShouldTurnAModuleInactive()
    {
        var module = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 120);

        module.Deactivate();

        module.IsActive.ShouldBeFalse();
    }

    [Test]
    public void ShouldLetTheMasterSwitchACoreModuleOffAndBackOn()
    {
        var module = PortalModule.Create(ModuleCode.Students, "Students", "", isCore: true, sortOrder: 50, isDefaultEnabled: true);

        module.Deactivate();
        module.IsActive.ShouldBeFalse();

        module.Activate();
        module.IsActive.ShouldBeTrue();
    }

    [Test]
    public void ShouldStillNotDeleteAModuleWithoutKeepingItsRow()
    {
        var module = PortalModule.Create(ModuleCode.Students, "Students", "", isCore: true, sortOrder: 50);

        module.Delete(DateTimeOffset.UtcNow, "user");

        module.IsDeleted.ShouldBeTrue();
        module.IsActive.ShouldBeFalse();
    }

    [Test]
    public void ShouldKeepTheDefaultFlagAndLetTheMasterChangeIt()
    {
        var module = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 120);
        module.IsDefaultEnabled.ShouldBeFalse();

        module.Update("Fees", "", isCore: false, sortOrder: 120, isDefaultEnabled: true);

        module.IsDefaultEnabled.ShouldBeTrue();
    }

    [Test]
    public void ShouldAllowAnInactiveModuleToBecomeCore()
    {
        var module = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 120);
        module.Deactivate();

        module.Update("Fees", "", isCore: true, sortOrder: 120);

        module.IsCore.ShouldBeTrue();
        module.IsActive.ShouldBeFalse();
    }

    [Test]
    public void ShouldLetAPaidModuleBecomeCoreAndBack()
    {
        var module = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 120);

        module.Update(" Fees and dues ", " Fee collection. ", isCore: true, sortOrder: 5);
        module.ShouldSatisfyAllConditions(
            m => m.Name.ShouldBe("Fees and dues"),
            m => m.Description.ShouldBe("Fee collection."),
            m => m.IsCore.ShouldBeTrue(),
            m => m.SortOrder.ShouldBe(5));

        module.Update("Fees", "", isCore: false, sortOrder: 120);
        module.Deactivate();
        module.IsActive.ShouldBeFalse();
    }

    [TestCase(ModuleCode.Fees)]
    [TestCase(ModuleCode.HrPayroll)]
    [TestCase(ModuleCode.School)]
    public void ShouldAcceptAModuleCode(ModuleCode code)
    {
        PortalModule.Create(code, "Module", "", isCore: false, sortOrder: 0).Code.ShouldBe(code);
    }

    [TestCase(ModuleCode.None)]
    [TestCase((ModuleCode)99)]
    public void ShouldRefuseAnUnknownModuleCode(ModuleCode code)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PortalModule.Create(code, "Module", "", isCore: false, sortOrder: 0));
    }

    [Test]
    public void ShouldRefuseANegativeSortOrderAndAnEmptyName()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: -1));
        Should.Throw<ArgumentException>(() => PortalModule.Create(ModuleCode.Fees, " ", "", isCore: false, sortOrder: 0));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            PortalModule.Create(ModuleCode.Fees, new string('a', PortalModule.NameMaxLength + 1), "", isCore: false, sortOrder: 0));
    }

    [Test]
    public void ShouldSeedEveryModuleCodeOnceWithTheAgreedCoreSet()
    {
        var codes = Enum.GetValues<ModuleCode>().Where(c => c != ModuleCode.None).ToList();
        var modules = ModuleCatalog.CreateModules();

        modules.Select(m => m.Code).ShouldBe(codes, ignoreOrder: true);
        modules.ShouldAllBe(m => m.IsActive);
        modules.Where(m => m.IsCore).Select(m => m.Code).ShouldBe(
            [ModuleCode.School, ModuleCode.Campus, ModuleCode.AcademicSetup, ModuleCode.UsersRoles, ModuleCode.Students, ModuleCode.Staff],
            ignoreOrder: true);
        modules.Select(m => m.SortOrder).ShouldBeUnique();
    }

    [Test]
    public void ShouldSeedDependenciesBetweenKnownModulesWithoutLoopsOrCoreToPaid()
    {
        var modules = ModuleCatalog.CreateModules().Select((m, index) => (Module: m, Id: index + 1)).ToDictionary(x => x.Module.Code);
        var pairs = ModuleCatalog.Dependencies.Select(d => (modules[d.Module].Id, modules[d.Requires].Id)).ToList();

        pairs.ShouldBeUnique();
        ModuleCatalog.Dependencies.ShouldNotContain(d => modules[d.Module].Module.IsCore && !modules[d.Requires].Module.IsCore);
        ModuleCatalog.Dependencies.ShouldContain((ModuleCode.Fees, ModuleCode.Students));

        for (var i = 0; i < pairs.Count; i++)
        {
            var others = pairs.Where((_, j) => j != i).ToList();
            ModuleDependencyGraph.WouldCreateCycle(others, pairs[i].Item1, pairs[i].Item2).ShouldBeFalse();
        }
    }
}
