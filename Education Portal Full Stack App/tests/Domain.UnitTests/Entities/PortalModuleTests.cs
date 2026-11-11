using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>Core modules are always on; default modules are on unless a school has its own choice; custom modules are off.</summary>
public class PortalModuleTests
{
    [TestCase(null, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldLetASchoolsChoiceWinOverADefaultModule(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Students, "Students", "", isCore: false, isDefault: true);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [TestCase(null, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldKeepACustomModuleOffUntilSwitchedOn(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Parents, "Parents", "", isCore: false, isDefault: false);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [TestCase(null)]
    [TestCase(false)]
    public void ShouldKeepACoreModuleOn(bool? choice)
    {
        var module = PortalModule.Create(ModuleKeys.Branches, "Branches", "", isCore: true, isDefault: false);

        module.IsDefault.ShouldBeTrue();
        module.IsEnabledFor(choice).ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseToMakeACoreModuleCustom()
    {
        var module = PortalModule.Create(ModuleKeys.Branches, "Branches", "", isCore: true, isDefault: true);

        Should.Throw<InvalidOperationException>(() => module.SetDefault(false));
    }

    [Test]
    public void ShouldSwitchADefaultModuleToCustom()
    {
        var module = PortalModule.Create(ModuleKeys.Grades, "Grades", "", isCore: false, isDefault: true);

        module.SetDefault(false);

        module.IsDefault.ShouldBeFalse();
        module.IsEnabledFor(null).ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAPerSchoolChoiceForACoreModule()
    {
        var module = PortalModule.Create(ModuleKeys.UsersAndRoles, "Users & Roles", "", isCore: true, isDefault: true);

        Should.Throw<InvalidOperationException>(() => SchoolModule.Create(1, module, isEnabled: false));
    }

    [Test]
    public void ShouldListEveryModuleKeyOnceInTheCatalog()
    {
        var keys = typeof(ModuleKeys).GetFields().Select(f => (string)f.GetValue(null)!).ToList();

        ModuleCatalog.CreateModules().Select(m => m.Key).ShouldBe(keys, ignoreOrder: true);
        ModuleCatalog.CreateModules().Where(m => m.IsCore).Select(m => m.Key)
            .ShouldBe([ModuleKeys.Branches, ModuleKeys.UsersAndRoles], ignoreOrder: true);
    }
}
