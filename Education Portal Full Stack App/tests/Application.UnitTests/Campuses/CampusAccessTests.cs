using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Campuses;

/// <summary>
/// Who may act on every campus without an assignment, and who fails closed. The assignments themselves are read from the
/// database and are covered by the functional tests (CampusAccessTests).
/// </summary>
public class CampusAccessTests
{
    private static CampusAccess Access(bool isMaster, string? userId, string[] roles, int? personId = null)
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);
        user.Setup(u => u.Roles).Returns(roles.ToList());
        var tenant = new Mock<ICurrentTenant>();
        tenant.Setup(t => t.IsMaster).Returns(isMaster);
        tenant.Setup(t => t.SchoolId).Returns(5);
        var identity = new Mock<IIdentityService>();
        identity.Setup(i => i.FindPersonIdOfAccountAsync(It.IsAny<string>())).ReturnsAsync(personId);

        return new CampusAccess(Mock.Of<IApplicationDbContext>(), user.Object, tenant.Object, identity.Object, Mock.Of<IRoleRights>());
    }

    [Test]
    public async Task ShouldLetTheMasterTenantAndAnAdministratorActOnEveryCampus()
    {
        (await Access(isMaster: true, "u1", []).GetAllowedCampusIdsAsync(CancellationToken.None)).ShouldBeNull();
        (await Access(isMaster: false, "u1", [Roles.Administrator]).GetAllowedCampusIdsAsync(CancellationToken.None)).ShouldBeNull();
        await Should.NotThrowAsync(() => Access(isMaster: false, "u1", [Roles.Administrator]).EnsureCampusAsync(42, CancellationToken.None));
    }

    [Test]
    public async Task ShouldGiveNoCampusToASignedOutUserOrAnAccountThatIsNoPersons()
    {
        (await Access(isMaster: false, userId: null, []).GetAllowedCampusIdsAsync(CancellationToken.None)).ShouldNotBeNull().ShouldBeEmpty();
        (await Access(isMaster: false, "u1", [], personId: null).GetAllowedCampusIdsAsync(CancellationToken.None)).ShouldNotBeNull().ShouldBeEmpty();
        await Should.ThrowAsync<ForbiddenAccessException>(() => Access(isMaster: false, "u1", []).EnsureCampusAsync(42, CancellationToken.None));
    }

    [Test]
    public async Task ShouldLetARowOfTheWholeSchoolPass() =>
        await Should.NotThrowAsync(() => Access(isMaster: false, "u1", []).EnsureCampusAsync(null, CancellationToken.None));
}
