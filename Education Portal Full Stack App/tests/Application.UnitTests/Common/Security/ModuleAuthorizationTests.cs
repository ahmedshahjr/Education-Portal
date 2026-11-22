using EducationPortal.Domain.Enums;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Security;

[Authorize(Module = ModuleCode.Exams)]
public sealed record GradesRequest;

[Authorize]
public sealed record NoModuleRequest;

/// <summary>
/// A request that belongs to a module runs only when the module is switched on for the school of the request's host
/// (SEC-AUTHZ-01, SEC-AUTHZ-06: a disabled module is 403). The master tenant is never limited by modules.
/// </summary>
public class ModuleAuthorizationTests
{
    private const int SchoolId = 7;

    private Mock<IUser> _user = null!;
    private Mock<ICurrentTenant> _tenant = null!;
    private Mock<IModuleAccess> _modules = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _user.Setup(u => u.Id).Returns("user-1");
        _tenant = new Mock<ICurrentTenant>();
        _tenant.Setup(t => t.SchoolId).Returns(SchoolId);
        _modules = new Mock<IModuleAccess>();
    }

    [Test]
    public async Task ShouldRunWhenTheModuleIsOnForTheSchool()
    {
        _modules.Setup(m => m.HasModuleAsync(SchoolId, ModuleCode.Exams, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(GradesRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseWhenTheModuleIsOffForTheSchool()
    {
        _modules.Setup(m => m.HasModuleAsync(SchoolId, ModuleCode.Exams, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var thrown = await Should.ThrowAsync<ModuleNotEnabledException>(() =>
            Authorizer().AuthorizeAsync(typeof(GradesRequest), CancellationToken.None));

        thrown.ModuleCode.ShouldBe(ModuleCode.Exams);
        thrown.ShouldBeAssignableTo<ForbiddenAccessException>(); // still a 403 for anything that handles forbidden requests
        ModuleNotEnabledException.ErrorCode.ShouldBe("MODULE_NOT_ENABLED");
    }

    [Test]
    public async Task ShouldRefuseWithoutAResolvedSchool()
    {
        _tenant.Setup(t => t.SchoolId).Returns((int?)null);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            Authorizer().AuthorizeAsync(typeof(GradesRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldNotLimitTheMasterTenant()
    {
        _tenant.Setup(t => t.IsMaster).Returns(true);

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(GradesRequest), CancellationToken.None));

        _modules.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldNotCheckModulesForARequestWithoutOne()
    {
        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(NoModuleRequest), CancellationToken.None));

        _modules.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldRequireSignInBeforeCheckingModules()
    {
        _user.Setup(u => u.Id).Returns((string?)null);

        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            Authorizer().AuthorizeAsync(typeof(GradesRequest), CancellationToken.None));

        _modules.VerifyNoOtherCalls();
    }

    private RequestAuthorizer Authorizer() =>
        new(_user.Object, Mock.Of<IIdentityService>(), _tenant.Object, _modules.Object, Mock.Of<IRoleRights>());
}
