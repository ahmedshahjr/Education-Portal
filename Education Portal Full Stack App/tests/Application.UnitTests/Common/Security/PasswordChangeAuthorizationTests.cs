using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Security;

[Authorize]
public sealed record OrdinaryRequest;

[Authorize(AllowWhilePasswordChangeRequired = true)]
public sealed record PasswordChangeRequest;

public sealed record AnonymousRequest;

/// <summary>
/// An account that still has its one-time password can only run requests marked as allowed (read itself, change the
/// password, sign out). Everything else is a 403 with the code PASSWORD_CHANGE_REQUIRED.
/// </summary>
public class PasswordChangeAuthorizationTests
{
    private Mock<IUser> _user = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _user.Setup(u => u.Id).Returns("user-1");
        _user.Setup(u => u.MustChangePassword).Returns(true);
    }

    [Test]
    public async Task ShouldRefuseAnOrdinaryRequestWhileThePasswordMustChange()
    {
        var thrown = await Should.ThrowAsync<PasswordChangeRequiredException>(() =>
            Authorizer().AuthorizeAsync(typeof(OrdinaryRequest), CancellationToken.None));

        thrown.ShouldBeAssignableTo<ForbiddenAccessException>();
        PasswordChangeRequiredException.ErrorCode.ShouldBe("PASSWORD_CHANGE_REQUIRED");
    }

    [Test]
    public async Task ShouldRunAnAllowedRequestWhileThePasswordMustChange()
    {
        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(PasswordChangeRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRunAnOrdinaryRequestOnceThePasswordIsChanged()
    {
        _user.Setup(u => u.MustChangePassword).Returns(false);

        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(OrdinaryRequest), CancellationToken.None));
    }

    [Test]
    public async Task ShouldLeaveRequestsWithoutAuthorizationAlone()
    {
        await Should.NotThrowAsync(() => Authorizer().AuthorizeAsync(typeof(AnonymousRequest), CancellationToken.None));
    }

    private RequestAuthorizer Authorizer() =>
        new(_user.Object, Mock.Of<IIdentityService>(), Mock.Of<ICurrentTenant>(), Mock.Of<IModuleAccess>(), Mock.Of<IRoleRights>());
}
