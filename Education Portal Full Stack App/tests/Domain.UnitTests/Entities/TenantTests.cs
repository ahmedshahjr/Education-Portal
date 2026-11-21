using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>ADR-021: a customer tenant, and exactly one master tenant that can never be switched off.</summary>
public class TenantTests
{
    [Test]
    public void ShouldCreateACustomerTenantWithATrimmedName()
    {
        var tenant = Tenant.Create("  Acme Group ");

        tenant.Name.ShouldBe("Acme Group");
        tenant.IsMaster.ShouldBeFalse();
    }

    [Test]
    public void ShouldCreateTheMasterTenantOnlyThroughCreateMaster()
    {
        Tenant.CreateMaster("Platform").IsMaster.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseAnEmptyName(string name) =>
        Should.Throw<ArgumentException>(() => Tenant.Create(name));

    [TestCase(Tenant.NameMaxLength, true)]
    [TestCase(Tenant.NameMaxLength + 1, false)]
    public void ShouldLimitTheNameLength(int length, bool valid)
    {
        var act = () => Tenant.Create(new string('a', length));

        if (valid) { act().Name.Length.ShouldBe(length); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldNeverDeactivateTheMasterTenant()
    {
        var master = Tenant.CreateMaster("Platform");

        Should.Throw<InvalidOperationException>(() => master.Deactivate());
        master.IsActive.ShouldBeTrue();
    }

    [Test]
    public void ShouldDeactivateACustomerTenant()
    {
        var tenant = Tenant.Create("Acme Group");

        tenant.Deactivate();

        tenant.IsActive.ShouldBeFalse();
    }

    [Test]
    public void ShouldLetASchoolJoinATenantOnlyWithAPositiveId()
    {
        var school = School.Create("Alpha", "alpha.example.com");

        Should.Throw<ArgumentOutOfRangeException>(() => school.AssignToTenant(0));
        school.AssignToTenant(7);
        school.TenantId.ShouldBe(7);
    }
}
