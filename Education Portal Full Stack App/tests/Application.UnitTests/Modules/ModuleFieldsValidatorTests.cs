using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>
/// The fields every module add or change sends (SEC-INPUT-01). The rules that read the catalogue (code taken, core
/// rules, loops) are covered against a real database in the functional tests.
/// </summary>
public class ModuleFieldsValidatorTests
{
    private sealed record Fields : ModuleFields;

    private sealed class FieldsValidator : ModuleFieldsValidator<Fields>;

    private readonly FieldsValidator _validator = new();

    [Test]
    public void ShouldAcceptAModule()
    {
        _validator.Validate(new Fields { Name = "Fees", Description = "", IsCore = false, SortOrder = 0 }).IsValid.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase(" ")]
    public void ShouldRequireAName(string name)
    {
        _validator.Validate(new Fields { Name = name }).Errors.ShouldContain(e => e.PropertyName == nameof(Fields.Name));
    }

    [TestCase(PortalModule.NameMaxLength, true)]
    [TestCase(PortalModule.NameMaxLength + 1, false)]
    public void ShouldLimitTheName(int length, bool valid)
    {
        _validator.Validate(new Fields { Name = new string('a', length) }).IsValid.ShouldBe(valid);
    }

    [TestCase(PortalModule.DescriptionMaxLength, true)]
    [TestCase(PortalModule.DescriptionMaxLength + 1, false)]
    public void ShouldLimitTheDescription(int length, bool valid)
    {
        _validator.Validate(new Fields { Name = "Fees", Description = new string('a', length) }).IsValid.ShouldBe(valid);
    }

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(int.MaxValue, true)]
    public void ShouldRefuseANegativeSortOrder(int sortOrder, bool valid)
    {
        _validator.Validate(new Fields { Name = "Fees", SortOrder = sortOrder }).IsValid.ShouldBe(valid);
    }
}
