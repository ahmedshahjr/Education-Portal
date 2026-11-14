using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class AddOnRequestTests
{
    private static PortalModule Module(bool isCore, int id)
    {
        var module = PortalModule.Create("FEES", "Fees", "", isCore, sortOrder: 0);
        module.Id = id;
        return module;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 13, 9, 0, 0, TimeSpan.Zero);

    [Test]
    public void ShouldStartPending()
    {
        var request = AddOnRequest.Create(9, Module(false, 7));

        request.ShouldSatisfyAllConditions(
            r => r.SchoolId.ShouldBe(9),
            r => r.ModuleId.ShouldBe(7),
            r => r.Status.ShouldBe(AddOnRequestStatus.Pending),
            r => r.DecidedAt.ShouldBeNull());
    }

    [Test]
    public void ShouldRefuseACoreModuleAnUnsavedModuleAndAnUnsavedSchool()
    {
        Should.Throw<ArgumentException>(() => AddOnRequest.Create(9, Module(true, 7)));
        Should.Throw<ArgumentOutOfRangeException>(() => AddOnRequest.Create(9, Module(false, 0)));
        Should.Throw<ArgumentOutOfRangeException>(() => AddOnRequest.Create(0, Module(false, 7)));
    }

    [Test]
    public void ShouldBeDecidedOnce()
    {
        var approved = AddOnRequest.Create(9, Module(false, 7));
        approved.Approve(Now);
        var rejected = AddOnRequest.Create(9, Module(false, 7));
        rejected.Reject(Now);

        approved.Status.ShouldBe(AddOnRequestStatus.Approved);
        approved.DecidedAt.ShouldBe(Now);
        rejected.Status.ShouldBe(AddOnRequestStatus.Rejected);
        Should.Throw<InvalidOperationException>(() => approved.Reject(Now));
        Should.Throw<InvalidOperationException>(() => rejected.Approve(Now));
    }
}
