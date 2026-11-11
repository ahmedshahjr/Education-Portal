using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>Default modules are on unless a school has its own choice; custom modules are off unless switched on.</summary>
public class PortalModuleTests
{
    [TestCase(null, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldLetASchoolsChoiceWinOverADefaultModule(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Students, "Students", "", isDefault: true);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [TestCase(null, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldKeepACustomModuleOffUntilSwitchedOn(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Parents, "Parents", "", isDefault: false);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [Test]
    public void ShouldSwitchADefaultModuleToCustom()
    {
        var module = PortalModule.Create(ModuleKeys.Grades, "Grades", "", isDefault: true);

        module.SetDefault(false);

        module.IsDefault.ShouldBeFalse();
        module.IsEnabledFor(null).ShouldBeFalse();
    }

    [Test]
    public void ShouldSwitchACustomModuleToDefault()
    {
        var module = PortalModule.Create(ModuleKeys.Branches, "Branches", "", isDefault: false);

        module.SetDefault(true);

        module.IsDefault.ShouldBeTrue();
        module.IsEnabledFor(null).ShouldBeTrue();
        module.IsEnabledFor(false).ShouldBeFalse();
    }

    [Test]
    public void ShouldListEveryModuleKeyOnceInTheCatalog()
    {
        var keys = typeof(ModuleKeys).GetFields().Select(f => (string)f.GetValue(null)!).ToList();

        ModuleCatalog.CreateModules().Select(m => m.Key).ShouldBe(keys, ignoreOrder: true);
    }
}
