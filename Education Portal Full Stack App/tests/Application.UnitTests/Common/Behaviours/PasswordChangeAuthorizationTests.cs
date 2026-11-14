using EducationPortal.Application.Common.Behaviours;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using MediatR;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Behaviours;

[Authorize]
public sealed record OrdinaryRequest : IRequest<int>;

[Authorize(AllowWhilePasswordChangeRequired = true)]
public sealed record PasswordChangeRequest : IRequest<int>;

public sealed record AnonymousRequest : IRequest<int>;

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
        var ran = false;

        var thrown = await Should.ThrowAsync<PasswordChangeRequiredException>(() =>
            Behaviour<OrdinaryRequest>().Handle(new OrdinaryRequest(), _ => { ran = true; return Task.FromResult(1); }, CancellationToken.None));

        ran.ShouldBeFalse();
        thrown.ShouldBeAssignableTo<ForbiddenAccessException>();
        PasswordChangeRequiredException.ErrorCode.ShouldBe("PASSWORD_CHANGE_REQUIRED");
    }

    [Test]
    public async Task ShouldRunAnAllowedRequestWhileThePasswordMustChange()
    {
        var result = await Behaviour<PasswordChangeRequest>().Handle(new PasswordChangeRequest(), _ => Task.FromResult(1), CancellationToken.None);

        result.ShouldBe(1);
    }

    [Test]
    public async Task ShouldRunAnOrdinaryRequestOnceThePasswordIsChanged()
    {
        _user.Setup(u => u.MustChangePassword).Returns(false);

        var result = await Behaviour<OrdinaryRequest>().Handle(new OrdinaryRequest(), _ => Task.FromResult(1), CancellationToken.None);

        result.ShouldBe(1);
    }

    [Test]
    public async Task ShouldLeaveRequestsWithoutAuthorizationAlone()
    {
        var result = await Behaviour<AnonymousRequest>().Handle(new AnonymousRequest(), _ => Task.FromResult(1), CancellationToken.None);

        result.ShouldBe(1);
    }

    private AuthorizationBehaviour<TRequest, int> Behaviour<TRequest>() where TRequest : notnull =>
        new(_user.Object, Mock.Of<IIdentityService>(), Mock.Of<ICurrentTenant>(), Mock.Of<IModuleAccess>());
}
