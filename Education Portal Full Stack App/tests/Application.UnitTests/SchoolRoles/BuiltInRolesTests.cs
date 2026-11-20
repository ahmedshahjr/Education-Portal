using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.SchoolRoles;

/// <summary>The four built-in roles: one per kind of user, each module at most once, and a parent only views.</summary>
public class BuiltInRolesTests
{
    [Test]
    public void ShouldHaveOneRoleForEachKindOfUser()
    {
        BuiltInRoles.All.Select(r => r.Name).ShouldBe(new[] { "Principal", "Teacher", "Student", "Parent" });
    }

    [Test]
    public void ShouldGiveThePrincipalEveryModuleInFull()
    {
        var principal = BuiltInRoles.All.Single(r => r.Name == BuiltInRoles.Principal).Permissions;

        principal.Select(p => p.Module).ShouldBe(Enum.GetValues<ModuleCode>().Where(m => m != ModuleCode.None), ignoreOrder: true);
        principal.ShouldAllBe(p => p.CanView && p.CanCreate && p.CanEdit && p.CanDelete && p.CanApprove && p.CanExport);
    }

    [Test]
    public void ShouldListEachModuleOnlyOncePerRole()
    {
        foreach (var (name, permissions) in BuiltInRoles.All)
        {
            permissions.Select(p => p.Module).Distinct().Count().ShouldBe(permissions.Count, customMessage: name);
        }
    }

    [Test]
    public void ShouldLetAParentOnlyView()
    {
        var parent = BuiltInRoles.All.Single(r => r.Name == BuiltInRoles.Parent).Permissions;

        parent.ShouldNotBeEmpty();
        parent.ShouldAllBe(p => p.CanView && !p.CanCreate && !p.CanEdit && !p.CanDelete && !p.CanApprove && !p.CanExport);
    }
}
