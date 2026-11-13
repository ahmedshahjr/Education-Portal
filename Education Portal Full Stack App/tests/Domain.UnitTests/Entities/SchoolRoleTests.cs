using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolRoleTests
{
    private static SchoolRole Role() { var r = SchoolRole.Create(4, "Registrar"); r.Id = 7; return r; }

    private static PortalModule Module() { var m = PortalModule.Create("FEES", "Fees", "", isCore: false, sortOrder: 0); m.Id = 9; return m; }

    [Test]
    public void ShouldCreateARoleForTheSchoolWithATrimmedName()
    {
        var role = SchoolRole.Create(4, "  Registrar ");

        role.SchoolId.ShouldBe(4);
        role.Name.ShouldBe("Registrar");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldRefuseAnInvalidSchool(int schoolId) =>
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolRole.Create(schoolId, "Registrar"));

    [TestCase("")]
    [TestCase("  ")]
    public void ShouldRefuseAnEmptyName(string name) =>
        Should.Throw<ArgumentException>(() => SchoolRole.Create(4, name));

    [TestCase(SchoolRole.NameMaxLength, true)]
    [TestCase(SchoolRole.NameMaxLength + 1, false)]
    public void ShouldLimitTheNameLength(int length, bool valid)
    {
        var act = () => SchoolRole.Create(4, new string('a', length));

        if (valid) { act().Name.Length.ShouldBe(length); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldRename()
    {
        var role = Role();
        role.Rename(" Head ");
        role.Name.ShouldBe("Head");
    }

    [Test]
    public void ShouldGivePermissionsTheRolesSchoolAndIds()
    {
        var permission = RolePermission.Create(Role(), Module(), true, true, false, false);

        permission.ShouldSatisfyAllConditions(
            p => p.SchoolId.ShouldBe(4),
            p => p.SchoolRoleId.ShouldBe(7),
            p => p.ModuleId.ShouldBe(9),
            p => p.CanView.ShouldBeTrue(),
            p => p.CanCreate.ShouldBeTrue(),
            p => p.CanEdit.ShouldBeFalse(),
            p => p.CanDelete.ShouldBeFalse());
    }

    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    public void ShouldRefuseRightsWithoutView(bool create, bool edit, bool delete) =>
        Should.Throw<ArgumentException>(() => RolePermission.Create(Role(), Module(), false, create, edit, delete));

    [Test]
    public void ShouldBeEmptyOnlyWithoutView()
    {
        var permission = RolePermission.Create(Role(), Module(), false, false, false, false);
        permission.IsEmpty.ShouldBeTrue();

        permission.Set(true, false, false, true);
        permission.IsEmpty.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAnUnsavedRoleOrModule()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RolePermission.Create(SchoolRole.Create(4, "A"), Module(), true, false, false, false));
        Should.Throw<ArgumentOutOfRangeException>(() => RolePermission.Create(Role(), PortalModule.Create("FEES", "Fees", "", false, 0), true, false, false, false));
    }
}
