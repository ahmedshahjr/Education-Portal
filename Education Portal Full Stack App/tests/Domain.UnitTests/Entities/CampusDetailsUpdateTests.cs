using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>Updating a campus's details replaces every field, including its phone numbers, in order.</summary>
public class CampusDetailsUpdateTests
{
    private static CampusDetails Details(params string[] phones) =>
        CampusDetails.Create("12 Main Road", 3, 5, "75500", phones, "https://karachi.example.com", "SN-1", "KHI", 9, 24.8607m, 67.0011m);

    [Test]
    public void ShouldReplaceEveryDetailAndItsPhoneNumbersInOrder()
    {
        var campus = Campus.Create(1, "Karachi", "0002");

        campus.UpdateDetails(Details("+92 21 1111111", "+92 21 2222222"));
        campus.UpdateDetails(Details("+92 21 3333333"));

        campus.ShouldSatisfyAllConditions(
            c => c.Address.ShouldBe("12 Main Road"),
            c => c.StateId.ShouldBe(3),
            c => c.CityId.ShouldBe(5),
            c => c.PostalCode.ShouldBe("75500"),
            c => c.Website.ShouldBe("https://karachi.example.com"),
            c => c.SchoolNumber.ShouldBe("SN-1"),
            c => c.ShortName.ShouldBe("KHI"),
            c => c.PrincipalUserDetailsId.ShouldBe(9),
            c => c.Phones.Select(p => p.Number).ShouldBe(["+92 21 3333333"]),
            c => c.Phones.Select(p => p.Position).ShouldBe([0]));
    }

    [Test]
    public void ShouldKeepThePhoneNumbersInTheOrderGiven()
    {
        var campus = Campus.Create(1, "Karachi", "0002");

        campus.UpdateDetails(Details("+92 21 1111111", "+92 21 2222222", "+92 21 3333333"));

        campus.Phones.Select(p => p.Number).ShouldBe(["+92 21 1111111", "+92 21 2222222", "+92 21 3333333"]);
        campus.Phones.Select(p => p.Position).ShouldBe([0, 1, 2]);
    }

    [Test]
    public void ShouldRefuseDetailsForADeletedCampus()
    {
        var campus = Campus.Create(1, "Karachi", "0002");
        campus.Delete(DateTimeOffset.UtcNow, "admin");

        Should.Throw<InvalidOperationException>(() => campus.UpdateDetails(Details("+92 21 1111111")));
    }

    [Test]
    public void ShouldRefuseMissingDetails()
    {
        var campus = Campus.Create(1, "Karachi", "0002");

        Should.Throw<ArgumentNullException>(() => campus.UpdateDetails(null!));
    }
}
