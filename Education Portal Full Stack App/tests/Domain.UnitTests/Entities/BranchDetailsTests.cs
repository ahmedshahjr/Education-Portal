using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class BranchDetailsTests
{
    [Test]
    public void ShouldReplaceEveryDetailWhenUpdated()
    {
        var branch = Branch.Create(1, "Karachi", "0002");

        branch.UpdateDetails(CampusDetails.Create("12 Main Road", 3, 5, "75500", "+92 21 1234567", "https://karachi.example.com", "SN-1", "KHI", 9));

        branch.Address.ShouldBe("12 Main Road");
        branch.StateId.ShouldBe(3);
        branch.CityId.ShouldBe(5);
        branch.PostalCode.ShouldBe("75500");
        branch.Phone.ShouldBe("+92 21 1234567");
        branch.Website.ShouldBe("https://karachi.example.com");
        branch.SchoolNumber.ShouldBe("SN-1");
        branch.ShortName.ShouldBe("KHI");
        branch.PrincipalUserDetailsId.ShouldBe(9);
    }

    [Test]
    public void ShouldClearTheDetailsWhenUpdatedWithEmptyOnes()
    {
        var branch = Branch.Create(1, "Karachi", "0002");
        branch.UpdateDetails(CampusDetails.Create("12 Main Road", null, null, null, null, null, null, null, null));

        branch.UpdateDetails(CampusDetails.Create(null, null, null, null, null, null, null, null, null));

        branch.Address.ShouldBeNull();
    }

    [Test]
    public void ShouldRefuseDetailsForADeletedCampus()
    {
        var branch = Branch.Create(1, "Karachi", "0002");
        branch.Delete(DateTimeOffset.UtcNow, "admin");

        Should.Throw<InvalidOperationException>(() => branch.UpdateDetails(CampusDetails.Create(null, null, null, null, null, null, null, null, null)));
    }

    [Test]
    public void ShouldRefuseMissingDetails()
    {
        var branch = Branch.Create(1, "Karachi", "0002");

        Should.Throw<ArgumentNullException>(() => branch.UpdateDetails(null!));
    }
}
