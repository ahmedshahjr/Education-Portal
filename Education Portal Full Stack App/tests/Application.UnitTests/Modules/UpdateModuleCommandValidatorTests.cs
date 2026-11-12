using EducationPortal.Application.Modules.Commands.UpdateModule;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>A module change names an existing key shape and one of the two module types (SEC-INPUT-01).</summary>
public class UpdateModuleCommandValidatorTests
{
    private readonly UpdateModuleCommandValidator _validator = new();

    [TestCase(ModuleType.Default, true)]
    [TestCase(ModuleType.Custom, false)]
    public void ShouldAcceptEitherTypeActiveOrNot(ModuleType type, bool isActive)
    {
        _validator.Validate(new UpdateModuleCommand { Key = "Grades", Type = type, IsActive = isActive }).IsValid.ShouldBeTrue();
    }

    [TestCase(0)]
    [TestCase(3)]
    public void ShouldRefuseAnUnknownType(int type)
    {
        var result = _validator.Validate(new UpdateModuleCommand { Key = "Grades", Type = (ModuleType)type, IsActive = true });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateModuleCommand.Type));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void ShouldRequireAKey(string key)
    {
        _validator.Validate(new UpdateModuleCommand { Key = key, Type = ModuleType.Default }).IsValid.ShouldBeFalse();
    }

    [TestCase(PortalModule.KeyMaxLength, true)]
    [TestCase(PortalModule.KeyMaxLength + 1, false)]
    public void ShouldLimitTheKeyLength(int length, bool valid)
    {
        _validator.Validate(new UpdateModuleCommand { Key = new string('a', length), Type = ModuleType.Default }).IsValid.ShouldBe(valid);
    }
}
