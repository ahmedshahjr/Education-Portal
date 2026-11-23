using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Security;

[Authorize(Module = ModuleCode.Exams, Right = AccessRight.Edit)]
public sealed record EditScoresRequest;

[Authorize(Right = AccessRight.View)]
public sealed record RightWithoutModuleRequest;

/// <summary>
/// A request that names a right runs only when one of the user's school roles gives it in the module (SEC-AUTHZ-02).
/// Administrators and the master tenant are not checked; a disabled module is refused before the right is looked at.
/// </summary>
public class RoleRightsAuthorizationTests
{
    private const int SchoolId = 7;

    private Mock<IUser> _user = null!;
    private Mock<ICurrentTenant> _tenant = null!;
    private Mock<IModuleAccess> _modules = null!;
    private Mock<IRoleRights> _rights = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _user.Setup(u => u.Id).Returns("teacher-1");
        _user.Setup(u => u.Roles).Returns(new List<string> { "Teacher" });
        _tenant = new Mock<ICurrentTenant>();
        _tenant.Setup(t => t.IsResolved).Returns(true);
        _tenant.Setup(t => t.SchoolId).Returns(SchoolId);
        _modules = new Mock<IModuleAccess>();
        _modules.Setup(m => m.HasModuleAsync(SchoolId, ModuleCode.Exams, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _rights = new Mock<IRoleRights>();
    }

    [Test]
    public async Task ShouldRunWhenARoleOfTheSchoolGivesTheRight()
    {
        _rights.Setup(r => r.HasRightAsync("teacher-1", SchoolId, ModuleCode.Exams, AccessRight.Edit, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseWhenNoRoleGivesTheRight()
    {
        _rights.Setup(r => r.HasRightAsync("teacher-1", SchoolId, ModuleCode.Exams, AccessRight.Edit, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldCheckTheRightInTheSchoolOfTheHostOnly()
    {
        _rights.Setup(r => r.HasRightAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ModuleCode>(), It.IsAny<AccessRight>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None);

        _rights.Verify(r => r.HasRightAsync("teacher-1", SchoolId, ModuleCode.Exams, AccessRight.Edit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCheckRightsForAnAdministrator()
    {
        _user.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));

        _rights.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldNotCheckRightsForTheMasterTenant()
    {
        _tenant.Setup(t => t.IsMaster).Returns(true);

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));

        _rights.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldRefuseADisabledModuleBeforeLookingAtTheRight()
    {
        _modules.Setup(m => m.HasModuleAsync(SchoolId, ModuleCode.Exams, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Should.ThrowAsync<ModuleNotEnabledException>(() =>
            Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));

        _rights.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldRefuseARightThatNamesNoModule()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            Authorizer().AuthorizeAsync(typeof(RightWithoutModuleRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseWithoutAResolvedSchool()
    {
        _tenant.Setup(t => t.IsResolved).Returns(false);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            Authorizer().AuthorizeAsync(typeof(EditScoresRequest), CancellationToken.None));
    }

    private RequestAuthorizer Authorizer() =>
        new(_user.Object, Mock.Of<IIdentityService>(), _tenant.Object, _modules.Object, _rights.Object);
}
