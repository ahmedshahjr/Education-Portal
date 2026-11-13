using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>The fields every role add or change sends (SEC-INPUT-01): name, each module once, rights need view, enabled modules only.</summary>
public class SchoolRoleFieldsValidatorTests
{
    private sealed record Fields : SchoolRoleFields;

    private sealed class FieldsValidator(IModuleAccess access, ICurrentTenant tenant) : SchoolRoleFieldsValidator<Fields>(access, tenant);

    private static FieldsValidator Validator(params string[] enabled)
    {
        var access = new Mock<IModuleAccess>();
        access.Setup(a => a.GetEnabledCodesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(enabled);
        var tenant = new Mock<ICurrentTenant>();
        tenant.Setup(t => t.SchoolId).Returns(5);
        return new FieldsValidator(access.Object, tenant.Object);
    }

    private static RolePermissionDto Right(string module, bool view = true, bool create = false) => new(module, view, create, false, false);

    [Test]
    public async Task ShouldAcceptARoleWithRightsInEnabledModules()
    {
        var result = await Validator("STUDENTS", "FEES").ValidateAsync(new Fields { Name = "Registrar", Permissions = [Right("STUDENTS"), Right("FEES", create: true)] });
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptARoleWithoutRights()
    {
        var result = await Validator().ValidateAsync(new Fields { Name = "Registrar" });
        result.IsValid.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task ShouldRequireAName(string name)
    {
        var result = await Validator().ValidateAsync(new Fields { Name = name });
        result.Errors.ShouldContain(e => e.PropertyName == nameof(Fields.Name));
    }

    [TestCase(SchoolRole.NameMaxLength, true)]
    [TestCase(SchoolRole.NameMaxLength + 1, false)]
    public async Task ShouldLimitTheName(int length, bool valid)
    {
        var result = await Validator().ValidateAsync(new Fields { Name = new string('a', length) });
        result.IsValid.ShouldBe(valid);
    }

    [Test]
    public async Task ShouldRefuseRightsWithoutView()
    {
        var result = await Validator("STUDENTS").ValidateAsync(new Fields { Name = "A", Permissions = [Right("STUDENTS", view: false, create: true)] });
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseAModuleListedTwice()
    {
        var result = await Validator("STUDENTS").ValidateAsync(new Fields { Name = "A", Permissions = [Right("STUDENTS"), Right("STUDENTS")] });
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseAModuleTheSchoolDoesNotHave()
    {
        var result = await Validator("STUDENTS").ValidateAsync(new Fields { Name = "A", Permissions = [Right("FEES")] });
        result.IsValid.ShouldBeFalse();
    }
}
