using EducationPortal.Domain.Enums;
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

    private static FieldsValidator Validator(params ModuleCode[] enabled)
    {
        var access = new Mock<IModuleAccess>();
        access.Setup(a => a.GetEnabledCodesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(enabled);
        var tenant = new Mock<ICurrentTenant>();
        tenant.Setup(t => t.SchoolId).Returns(5);
        return new FieldsValidator(access.Object, tenant.Object);
    }

    private static RolePermissionDto Right(ModuleCode module, bool view = true, bool create = false) => new(module, view, create, false, false, false, false);

    [Test]
    public async Task ShouldAcceptARoleWithRightsInEnabledModules()
    {
        var result = await Validator(ModuleCode.Students, ModuleCode.Fees).ValidateAsync(new Fields { Name = "Registrar", Permissions = [Right(ModuleCode.Students), Right(ModuleCode.Fees, create: true)] });
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
        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [Right(ModuleCode.Students, view: false, create: true)] });
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseAModuleListedTwice()
    {
        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [Right(ModuleCode.Students), Right(ModuleCode.Students)] });
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseAModuleTheSchoolDoesNotHave()
    {
        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [Right(ModuleCode.Fees)] });
        result.IsValid.ShouldBeFalse();
    }

    private static RolePermissionDto OnCampuses(ModuleCode module, params Guid[] campusIds) =>
        new(module, true, false, false, false, false, false, false, AllCampuses: false, CampusIds: campusIds);

    [Test]
    public async Task ShouldRefuseAViewRightWithNoCampusWhenNotForAllCampuses()
    {
        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [OnCampuses(ModuleCode.Students)] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(Fields.Permissions));
    }

    [Test]
    public async Task ShouldAcceptAViewRightOnOneCampus()
    {
        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [OnCampuses(ModuleCode.Students, Guid.NewGuid())] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptNoViewRightWithNoCampusWhenNotForAllCampuses()
    {
        var noRights = new RolePermissionDto(ModuleCode.Students, false, false, false, false, false, false, false, AllCampuses: false, CampusIds: []);

        var result = await Validator(ModuleCode.Students).ValidateAsync(new Fields { Name = "A", Permissions = [noRights] });

        result.IsValid.ShouldBeTrue();
    }
}
