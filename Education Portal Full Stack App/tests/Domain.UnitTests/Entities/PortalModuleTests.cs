using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>
/// A module is core (in every subscription, always on) or paid (on for a school only when given to it), and an inactive
/// module is off for every school.
/// </summary>
public class PortalModuleTests
{
    [Test]
    public void ShouldTurnAModuleInactive()
    {
        var module = PortalModule.Create(ModuleCodes.Fees, "Fees", "", isCore: false, sortOrder: 120);

        module.Deactivate();

        module.IsActive.ShouldBeFalse();
    }

    [Test]
    public void ShouldNeverDeactivateOrDeleteACoreModule()
    {
        var module = PortalModule.Create(ModuleCodes.Students, "Students", "", isCore: true, sortOrder: 50);

        Should.Throw<InvalidOperationException>(module.Deactivate);
        Should.Throw<InvalidOperationException>(() => module.Delete(DateTimeOffset.UtcNow, "user"));
        module.IsActive.ShouldBeTrue();
        module.IsDeleted.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseToMakeAnInactiveModuleCore()
    {
        var module = PortalModule.Create(ModuleCodes.Fees, "Fees", "", isCore: false, sortOrder: 120);
        module.Deactivate();

        Should.Throw<InvalidOperationException>(() => module.Update("Fees", "", isCore: true, sortOrder: 120));
        module.IsCore.ShouldBeFalse();
    }

    [Test]
    public void ShouldLetAPaidModuleBecomeCoreAndBack()
    {
        var module = PortalModule.Create(ModuleCodes.Fees, "Fees", "", isCore: false, sortOrder: 120);

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

    [TestCase("FEES")]
    [TestCase("HR_PAYROLL")]
    [TestCase("A")]
    [TestCase("L2")]
    public void ShouldAcceptACode(string code)
    {
        PortalModule.Create(code, "Module", "", isCore: false, sortOrder: 0).Code.ShouldBe(code);
    }

    [TestCase("")]
    [TestCase("fees")]
    [TestCase("Fees")]
    [TestCase("2FA")]
    [TestCase("_FEES")]
    [TestCase("FEES-2")]
    [TestCase("FEES ")]
    public void ShouldRefuseABadCode(string code)
    {
        PortalModule.IsValidCode(code).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => PortalModule.Create(code, "Module", "", isCore: false, sortOrder: 0));
    }

    [TestCase(PortalModule.CodeMaxLength, true)]
    [TestCase(PortalModule.CodeMaxLength + 1, false)]
    public void ShouldLimitTheCodeLength(int length, bool valid)
    {
        PortalModule.IsValidCode(new string('A', length)).ShouldBe(valid);
    }

    [Test]
    public void ShouldRefuseANegativeSortOrderAndAnEmptyName()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PortalModule.Create("FEES", "Fees", "", isCore: false, sortOrder: -1));
        Should.Throw<ArgumentException>(() => PortalModule.Create("FEES", " ", "", isCore: false, sortOrder: 0));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            PortalModule.Create("FEES", new string('a', PortalModule.NameMaxLength + 1), "", isCore: false, sortOrder: 0));
    }

    [Test]
    public void ShouldSeedEveryModuleCodeOnceWithTheAgreedCoreSet()
    {
        var codes = typeof(ModuleCodes).GetFields().Select(f => (string)f.GetValue(null)!).ToList();
        var modules = ModuleCatalog.CreateModules();

        modules.Select(m => m.Code).ShouldBe(codes, ignoreOrder: true);
        modules.ShouldAllBe(m => m.IsActive);
        modules.Where(m => m.IsCore).Select(m => m.Code).ShouldBe(
            [ModuleCodes.School, ModuleCodes.Branch, ModuleCodes.AcademicSetup, ModuleCodes.UsersRoles, ModuleCodes.Students, ModuleCodes.Staff],
            ignoreOrder: true);
        modules.Select(m => m.SortOrder).ShouldBeUnique();
    }

    [Test]
    public void ShouldSeedDependenciesBetweenKnownModulesWithoutLoopsOrCoreToPaid()
    {
        var modules = ModuleCatalog.CreateModules().Select((m, index) => (Module: m, Id: index + 1)).ToDictionary(x => x.Module.Code);
        var pairs = ModuleCatalog.Dependencies.Select(d => (modules[d.ModuleCode].Id, modules[d.RequiresCode].Id)).ToList();

        pairs.ShouldBeUnique();
        ModuleCatalog.Dependencies.ShouldNotContain(d => modules[d.ModuleCode].Module.IsCore && !modules[d.RequiresCode].Module.IsCore);
        ModuleCatalog.Dependencies.ShouldContain((ModuleCodes.Fees, ModuleCodes.Students));

        for (var i = 0; i < pairs.Count; i++)
        {
            var others = pairs.Where((_, j) => j != i).ToList();
            ModuleDependencyGraph.WouldCreateCycle(others, pairs[i].Item1, pairs[i].Item2).ShouldBeFalse();
        }
    }
}
