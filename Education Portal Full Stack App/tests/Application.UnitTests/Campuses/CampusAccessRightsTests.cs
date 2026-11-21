using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Campuses;

/// <summary>
/// The campus-aware overloads: a campus counts only when the user is assigned to it and a role gives the right there.
/// Cases that need assignment rows read the database and are covered by the functional tests (CampusAccessTests).
/// </summary>
public class CampusAccessRightsTests
{
    private const ModuleCode Module = ModuleCode.Students;
    private const AccessRight Right = AccessRight.Edit;

    private static (CampusAccess Access, Mock<IRoleRights> Roles) Access(bool isMaster, string? userId, string[] roles, int? schoolId = 5)
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);
        user.Setup(u => u.Roles).Returns(roles.ToList());
        var tenant = new Mock<ICurrentTenant>();
        tenant.Setup(t => t.IsMaster).Returns(isMaster);
        tenant.Setup(t => t.SchoolId).Returns(schoolId);
        var roleRights = new Mock<IRoleRights>();

        var access = new CampusAccess(Mock.Of<IApplicationDbContext>(), user.Object, tenant.Object, Mock.Of<IIdentityService>(), roleRights.Object);
        return (access, roleRights);
    }

    [Test]
    public async Task ShouldLetTheMasterTenantAndAnAdministratorSkipTheRoleLookup()
    {
        var master = Access(isMaster: true, "u1", []);
        (await master.Access.GetAllowedCampusIdsAsync(Module, Right, CancellationToken.None)).ShouldBeNull();
        master.Roles.Verify(r => r.GetRightCampusIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ModuleCode>(), It.IsAny<AccessRight>(), It.IsAny<CancellationToken>()), Times.Never);

        var admin = Access(isMaster: false, "u1", [Roles.Administrator]);
        (await admin.Access.GetAllowedCampusIdsAsync(Module, Right, CancellationToken.None)).ShouldBeNull();
        admin.Roles.Verify(r => r.GetRightCampusIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ModuleCode>(), It.IsAny<AccessRight>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldGiveNoCampusWhenThereIsNoUserOrNoSchool()
    {
        var signedOut = Access(isMaster: false, userId: null, []);
        (await signedOut.Access.GetAllowedCampusIdsAsync(Module, Right, CancellationToken.None)).ShouldNotBeNull().ShouldBeEmpty();

        var noSchool = Access(isMaster: false, "u1", [], schoolId: null);
        (await noSchool.Access.GetAllowedCampusIdsAsync(Module, Right, CancellationToken.None)).ShouldNotBeNull().ShouldBeEmpty();
        noSchool.Roles.Verify(r => r.GetRightCampusIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ModuleCode>(), It.IsAny<AccessRight>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldPassAnyCampusForTheMasterTenantAndAnAdministrator()
    {
        await Should.NotThrowAsync(() => Access(isMaster: true, "u1", []).Access.EnsureCampusAsync(42, Module, Right, CancellationToken.None));
        await Should.NotThrowAsync(() => Access(isMaster: false, "u1", [Roles.Administrator]).Access.EnsureCampusAsync(42, Module, Right, CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseACampusForASignedOutUser()
    {
        var signedOut = Access(isMaster: false, userId: null, []);
        await Should.ThrowAsync<ForbiddenAccessException>(() => signedOut.Access.EnsureCampusAsync(42, Module, Right, CancellationToken.None));
    }

    [Test]
    public async Task ShouldLetARowOfTheWholeSchoolPassWithoutAnyLookup()
    {
        var signedOut = Access(isMaster: false, userId: null, []);
        await Should.NotThrowAsync(() => signedOut.Access.EnsureCampusAsync(null, Module, Right, CancellationToken.None));
        signedOut.Roles.Verify(r => r.GetRightCampusIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ModuleCode>(), It.IsAny<AccessRight>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
