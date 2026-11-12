using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>
/// A module is Default (on unless a school has its own choice) or Custom (off unless switched on), and an inactive module
/// is off for every school.
/// </summary>
public class PortalModuleTests
{
    [TestCase(null, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldLetASchoolsChoiceWinOverADefaultModule(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Students, "Students", "", ModuleType.Default);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [TestCase(null, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldKeepACustomModuleOffUntilSwitchedOn(bool? choice, bool expected)
    {
        var module = PortalModule.Create(ModuleKeys.Parents, "Parents", "", ModuleType.Custom);

        module.IsEnabledFor(choice).ShouldBe(expected);
    }

    [TestCase(ModuleType.Default, null)]
    [TestCase(ModuleType.Default, true)]
    [TestCase(ModuleType.Custom, true)]
    public void ShouldTurnAnInactiveModuleOffForEverySchool(ModuleType type, bool? choice)
    {
        var module = PortalModule.Create(ModuleKeys.Grades, "Grades", "", type);

        module.Deactivate();

        module.IsActive.ShouldBeFalse();
        module.IsEnabledFor(choice).ShouldBeFalse();
    }

    [Test]
    public void ShouldStartActiveAndComeBackOnWhenActivated()
    {
        var module = PortalModule.Create(ModuleKeys.Grades, "Grades", "", ModuleType.Default);
        module.IsActive.ShouldBeTrue();

        module.Deactivate();
        module.Activate();

        module.IsEnabledFor(null).ShouldBeTrue();
    }

    [Test]
    public void ShouldChangeTheType()
    {
        var module = PortalModule.Create(ModuleKeys.Branches, "Branches", "", ModuleType.Default);

        module.SetType(ModuleType.Custom);

        module.Type.ShouldBe(ModuleType.Custom);
        module.IsEnabledFor(null).ShouldBeFalse();
    }

    [TestCase(0)]
    [TestCase(3)]
    public void ShouldRefuseAnUnknownType(int type)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PortalModule.Create(ModuleKeys.Grades, "Grades", "", (ModuleType)type));
    }

    [Test]
    public void ShouldListEveryModuleKeyOnceInTheCatalog()
    {
        var keys = typeof(ModuleKeys).GetFields().Select(f => (string)f.GetValue(null)!).ToList();

        ModuleCatalog.CreateModules().Select(m => m.Key).ShouldBe(keys, ignoreOrder: true);
        ModuleCatalog.CreateModules().ShouldAllBe(m => m.IsActive);
    }
}
