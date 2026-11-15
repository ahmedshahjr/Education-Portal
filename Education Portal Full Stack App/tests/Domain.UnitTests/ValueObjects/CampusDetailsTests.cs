using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.ValueObjects;

/// <summary>A campus's details: the required fields, one to five phone numbers, and an optional website and principal.</summary>
public class CampusDetailsTests
{
    private const string Address = "12 Main Road";
    private const string PostalCode = "75500";
    private const string Phone = "+92 21 1234567";
    private const string SchoolNumber = "SN-1";
    private const string ShortName = "KHI";

    [Test]
    public void ShouldTrimTheText_AndKeepThePhonesInTheOrderGiven()
    {
        var details = CampusDetails.Create("  12 Main Road  ", 3, 5, " 75500 ", [" +92 21 1234567 ", "0300-1234567"], "  https://school.example.com  ", " SN-1 ", " KHI ", null);

        details.ShouldSatisfyAllConditions(
            d => d.Address.ShouldBe("12 Main Road"),
            d => d.PostalCode.ShouldBe("75500"),
            d => d.Phones.ShouldBe(["+92 21 1234567", "0300-1234567"]),
            d => d.Website.ShouldBe("https://school.example.com"),
            d => d.SchoolNumber.ShouldBe("SN-1"),
            d => d.ShortName.ShouldBe("KHI"));
    }

    [Test]
    public void ShouldAcceptAWebsiteAndAPrincipalAsOptional()
    {
        var details = CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], null, SchoolNumber, ShortName, null);

        details.Website.ShouldBeNull();
        details.PrincipalUserDetailsId.ShouldBeNull();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankRequiredText(string blank)
    {
        Should.Throw<ArgumentException>(() => CampusDetails.Create(blank, 3, 5, PostalCode, [Phone], null, SchoolNumber, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, blank, [Phone], null, SchoolNumber, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], null, blank, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], null, SchoolNumber, blank, null));
    }

    [TestCase(0, 5)]
    [TestCase(3, 0)]
    [TestCase(-1, 5)]
    public void ShouldRefuseAStateOrCityThatIsNotSaved(int stateId, int cityId) =>
        Should.Throw<ArgumentOutOfRangeException>(() => CampusDetails.Create(Address, stateId, cityId, PostalCode, [Phone], null, SchoolNumber, ShortName, null));

    [Test]
    public void ShouldRefuseACampusWithoutAPhoneNumber()
    {
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, [], null, SchoolNumber, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, null, null, SchoolNumber, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, ["  "], null, SchoolNumber, ShortName, null));
    }

    [TestCase(1, true)]
    [TestCase(5, true)]
    [TestCase(6, false)]
    public void ShouldAcceptUpToFivePhoneNumbers(int count, bool accepted)
    {
        var phones = Enumerable.Range(0, count).Select(i => $"+92 21 000000{i}").ToList();
        Action create = () => CampusDetails.Create(Address, 3, 5, PostalCode, phones, null, SchoolNumber, ShortName, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentException>(create);
        }
    }

    [Test]
    public void ShouldRefuseAPhoneNumberThatIsNotValidOrListedTwice()
    {
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, ["123"], null, SchoolNumber, ShortName, null));
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone, Phone], null, SchoolNumber, ShortName, null));
    }

    [TestCase("ABCDE12345", true)]
    [TestCase("ABCDE123456", false)]
    [TestCase("12#45", false)]
    public void ShouldLimitThePostalCodeTo10CharactersOfLettersDigitsSpacesAndHyphens(string code, bool accepted)
    {
        Action create = () => CampusDetails.Create(Address, 3, 5, code, [Phone], null, SchoolNumber, ShortName, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentException>(create);
        }
    }

    [TestCase(CampusDetails.AddressMaxLength, true)]
    [TestCase(CampusDetails.AddressMaxLength + 1, false)]
    public void ShouldLimitTheAddress(int length, bool accepted)
    {
        Action create = () => CampusDetails.Create(new string('a', length), 3, 5, PostalCode, [Phone], null, SchoolNumber, ShortName, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase(CampusDetails.SchoolNumberMaxLength, true)]
    [TestCase(CampusDetails.SchoolNumberMaxLength + 1, false)]
    public void ShouldLimitTheSchoolNumber(int length, bool accepted)
    {
        Action create = () => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], null, new string('n', length), ShortName, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase(CampusDetails.ShortNameMaxLength, true)]
    [TestCase(CampusDetails.ShortNameMaxLength + 1, false)]
    public void ShouldLimitTheShortName(int length, bool accepted)
    {
        Action create = () => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], null, SchoolNumber, new string('s', length), null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase("ftp://school.example.com")]
    [TestCase("school.example.com")]
    public void ShouldRefuseAWebsiteThatIsNotHttp(string website) =>
        Should.Throw<ArgumentException>(() => CampusDetails.Create(Address, 3, 5, PostalCode, [Phone], website, SchoolNumber, ShortName, null));
}
