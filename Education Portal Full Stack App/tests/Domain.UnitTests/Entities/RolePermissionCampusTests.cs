using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>R1.1 and R1.2 (ADR-022): a permission belongs to a campus of the role's school, may apply to all campuses, and ViewHistory needs View.</summary>
public class RolePermissionCampusTests
{
    private static SchoolRole Role(int schoolId = TestCampuses.SchoolId) { var r = SchoolRole.Create(schoolId, "Registrar"); r.Id = 7; return r; }

    private static PortalModule Module() { var m = PortalModule.Create(ModuleCode.Fees, "Fees", "", isCore: false, sortOrder: 0); m.Id = 9; return m; }

    private static RolePermission Create(Campus? campus = null, bool allCampuses = false, bool view = true, bool viewHistory = false, SchoolRole? role = null) =>
        RolePermission.Create(role ?? Role(), Module(), campus ?? TestCampuses.MainCampus(), allCampuses, view, false, false, false, false, false, viewHistory);

    [Test]
    public void ShouldStoreTheCampusAndTheAllCampusesFlag()
    {
        var campus = TestCampuses.MainCampus();

        var one = Create(campus, allCampuses: false);
        var all = Create(campus, allCampuses: true);

        one.CampusId.ShouldBe(campus.Id);
        one.AllCampuses.ShouldBeFalse();
        all.CampusId.ShouldBe(campus.Id);
        all.AllCampuses.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseANullCampus() =>
        Should.Throw<ArgumentNullException>(() => RolePermission.Create(Role(), Module(), null!, false, true, false, false, false, false, false, false));

    [Test]
    public void ShouldRefuseACampusOfAnotherSchool()
    {
        var otherSchoolsRole = Role(schoolId: TestCampuses.SchoolId + 1);

        Should.Throw<ArgumentException>(() => Create(role: otherSchoolsRole));
    }

    [Test]
    public void ShouldRefuseAnUnsavedCampus()
    {
        var campus = TestCampuses.MainCampus();
        campus.Id = 0;

        Should.Throw<ArgumentOutOfRangeException>(() => Create(campus));
    }

    [Test]
    public void ShouldGiveViewHistoryOnlyWithView()
    {
        Create(view: true, viewHistory: true).CanViewHistory.ShouldBeTrue();
        Create(view: true, viewHistory: false).CanViewHistory.ShouldBeFalse();
        Should.Throw<ArgumentException>(() => Create(view: false, viewHistory: true));
    }

    [Test]
    public void ShouldRefuseSettingViewHistoryWithoutView()
    {
        var permission = Create();

        Should.Throw<ArgumentException>(() => permission.Set(false, false, false, false, false, false, true));
        permission.Set(true, false, false, false, false, false, true);
        permission.CanViewHistory.ShouldBeTrue();
    }

    [Test]
    public void ShouldNameViewHistoryAsRightSeven() =>
        ((int)AccessRight.ViewHistory).ShouldBe(7);
}
