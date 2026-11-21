using EducationPortal.Domain.Enums;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolRoleTests
{
    private static SchoolRole Role() { var r = SchoolRole.Create(4, "Registrar"); r.Id = 7; return r; }

    private static Campus MainCampus()
    {
        var campus = TestCampuses.MainCampus();
        typeof(Campus).GetProperty(nameof(Campus.SchoolId))!.SetValue(campus, 4);
        return campus;
    }

    private static PortalModule Module() { var m = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 0); m.Id = 9; return m; }

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
        var permission = RolePermission.Create(Role(), Module(), MainCampus(), false, true, true, false, false, true, false, false);

        permission.ShouldSatisfyAllConditions(
            p => p.SchoolId.ShouldBe(4),
            p => p.SchoolRoleId.ShouldBe(7),
            p => p.ModuleId.ShouldBe(9),
            p => p.CanView.ShouldBeTrue(),
            p => p.CanCreate.ShouldBeTrue(),
            p => p.CanEdit.ShouldBeFalse(),
            p => p.CanDelete.ShouldBeFalse(),
            p => p.CanApprove.ShouldBeTrue(),
            p => p.CanExport.ShouldBeFalse());
    }

    [TestCase(true, false, false, false, false)]
    [TestCase(false, true, false, false, false)]
    [TestCase(false, false, true, false, false)]
    [TestCase(false, false, false, true, false)]
    [TestCase(false, false, false, false, true)]
    public void ShouldRefuseRightsWithoutView(bool create, bool edit, bool delete, bool approve, bool export) =>
        Should.Throw<ArgumentException>(() => RolePermission.Create(Role(), Module(), MainCampus(), false, false, create, edit, delete, approve, export, false));

    [Test]
    public void ShouldBeEmptyOnlyWithoutView()
    {
        var permission = RolePermission.Create(Role(), Module(), MainCampus(), false, false, false, false, false, false, false, false);
        permission.IsEmpty.ShouldBeTrue();

        permission.Set(true, false, false, true, false, true, false);
        permission.IsEmpty.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAnUnsavedRoleOrModule()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RolePermission.Create(SchoolRole.Create(4, "A"), Module(), MainCampus(), false, true, false, false, false, false, false, false));
        Should.Throw<ArgumentOutOfRangeException>(() => RolePermission.Create(Role(), PortalModule.Create(ModuleCode.Fees, "Fees", "", false, 0), MainCampus(), false, true, false, false, false, false, false, false));
    }

    [Test]
    public void ShouldStoreATrimmedDescriptionAndBlankAsNull()
    {
        var role = SchoolRole.Create(4, "Registrar", "  Keeps the records ");
        role.Description.ShouldBe("Keeps the records");
        role.IsSystem.ShouldBeFalse();

        role.Describe("   ");
        role.Description.ShouldBeNull();
    }

    [TestCase(SchoolRole.DescriptionMaxLength, true)]
    [TestCase(SchoolRole.DescriptionMaxLength + 1, false)]
    public void ShouldLimitTheDescriptionLength(int length, bool valid)
    {
        var act = () => SchoolRole.Create(4, "Registrar", new string('a', length));

        if (valid) { act().Description!.Length.ShouldBe(length); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldKeepTheNameOfASystemRole()
    {
        var role = SchoolRole.CreateSystem(4, "Teacher");

        role.IsSystem.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => role.Rename("Tutor"));
        role.Rename(" Teacher ");
        role.Name.ShouldBe("Teacher");
    }
}
