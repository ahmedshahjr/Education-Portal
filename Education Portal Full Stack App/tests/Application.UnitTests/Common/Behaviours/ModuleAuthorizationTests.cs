using EducationPortal.Application.Common.Behaviours;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using MediatR;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Behaviours;

[Authorize(Module = ModuleCodes.Exams)]
public sealed record GradesRequest : IRequest<int>;

[Authorize]
public sealed record NoModuleRequest : IRequest<int>;

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
        _modules.Setup(m => m.IsEnabledAsync(SchoolId, ModuleCodes.Exams, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Behaviour<GradesRequest>().Handle(new GradesRequest(), _ => Task.FromResult(42), CancellationToken.None);

        result.ShouldBe(42);
    }

    [Test]
    public async Task ShouldRefuseWhenTheModuleIsOffForTheSchool()
    {
        _modules.Setup(m => m.IsEnabledAsync(SchoolId, ModuleCodes.Exams, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var ran = false;

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            Behaviour<GradesRequest>().Handle(new GradesRequest(), _ => { ran = true; return Task.FromResult(42); }, CancellationToken.None));

        ran.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseWithoutAResolvedSchool()
    {
        _tenant.Setup(t => t.SchoolId).Returns((int?)null);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            Behaviour<GradesRequest>().Handle(new GradesRequest(), _ => Task.FromResult(42), CancellationToken.None));
    }

    [Test]
    public async Task ShouldNotLimitTheMasterTenant()
    {
        _tenant.Setup(t => t.IsMaster).Returns(true);

        var result = await Behaviour<GradesRequest>().Handle(new GradesRequest(), _ => Task.FromResult(42), CancellationToken.None);

        result.ShouldBe(42);
        _modules.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldNotCheckModulesForARequestWithoutOne()
    {
        var result = await Behaviour<NoModuleRequest>().Handle(new NoModuleRequest(), _ => Task.FromResult(42), CancellationToken.None);

        result.ShouldBe(42);
        _modules.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldRequireSignInBeforeCheckingModules()
    {
        _user.Setup(u => u.Id).Returns((string?)null);

        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            Behaviour<GradesRequest>().Handle(new GradesRequest(), _ => Task.FromResult(42), CancellationToken.None));

        _modules.VerifyNoOtherCalls();
    }

    private AuthorizationBehaviour<TRequest, int> Behaviour<TRequest>() where TRequest : notnull =>
        new(_user.Object, Mock.Of<IIdentityService>(), _tenant.Object, _modules.Object);
}
