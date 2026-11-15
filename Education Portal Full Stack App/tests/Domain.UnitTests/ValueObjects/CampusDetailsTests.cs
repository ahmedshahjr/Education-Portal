using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.ValueObjects;

public class CampusDetailsTests
{
    [Test]
    public void ShouldTrimTheText_AndTurnBlanksIntoNull()
    {
        var details = Create(address: "  12 Main Road  ", postalCode: "   ", phone: " +92 300 1234567 ", website: "  https://school.example.com  ");

        details.Address.ShouldBe("12 Main Road");
        details.PostalCode.ShouldBeNull();
        details.Phone.ShouldBe("+92 300 1234567");
        details.Website.ShouldBe("https://school.example.com");
    }

    [Test]
    public void ShouldAcceptEveryFieldEmpty()
    {
        var details = Create();

        details.Address.ShouldBeNull();
        details.StateId.ShouldBeNull();
        details.CityId.ShouldBeNull();
        details.PrincipalUserDetailsId.ShouldBeNull();
    }

    [TestCase(CampusDetails.AddressMaxLength, true)]
    [TestCase(CampusDetails.AddressMaxLength + 1, false)]
    public void ShouldLimitTheAddressLength(int length, bool accepted)
    {
        Action create = () => Create(address: new string('a', length));

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase("ABCDE12345", true)]
    [TestCase("ABCDE123456", false)]
    public void ShouldLimitThePostalCodeTo10Characters(string code, bool accepted)
    {
        Action create = () => Create(postalCode: code);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase("12 34-5", true)]
    [TestCase("12#345", false)]
    [TestCase("-12345", false)]
    public void ShouldAcceptOnlyLettersDigitsSpacesAndHyphensInThePostalCode(string code, bool accepted)
    {
        Action create = () => Create(postalCode: code);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentException>(create);
        }
    }

    [TestCase("+92 300 1234567", true)]
    [TestCase("+123456789012345", true)]
    [TestCase("1234567", true)]
    [TestCase("123456", false)]
    [TestCase("+1234567890123456", false)]
    [TestCase("0300-abc-1234", false)]
    [TestCase("+(0)", false)]
    public void ShouldAcceptAPhoneWithSevenToFifteenDigits(string phone, bool accepted)
    {
        Action create = () => Create(phone: phone);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentException>(create);
        }
    }

    [TestCase("https://school.example.com", true)]
    [TestCase("http://school.example.com/about", true)]
    [TestCase("ftp://school.example.com", false)]
    [TestCase("school.example.com", false)]
    public void ShouldAcceptOnlyAnHttpOrHttpsWebsite(string website, bool accepted)
    {
        Action create = () => Create(website: website);

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
    public void ShouldRefuseACityWithoutAState()
    {
        Should.Throw<ArgumentException>(() => CampusDetails.Create(null, null, 5, null, null, null, null, null, null));
    }

    [Test]
    public void ShouldAcceptACityWithItsState()
    {
        var details = CampusDetails.Create(null, 3, 5, null, null, null, null, null, null);

        details.StateId.ShouldBe(3);
        details.CityId.ShouldBe(5);
    }

    [TestCase(CampusDetails.ShortNameMaxLength, true)]
    [TestCase(CampusDetails.ShortNameMaxLength + 1, false)]
    public void ShouldLimitTheShortName(int length, bool accepted)
    {
        Action create = () => CampusDetails.Create(null, null, null, null, null, null, null, new string('s', length), null);

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
        Action create = () => CampusDetails.Create(null, null, null, null, null, null, new string('n', length), null, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    private static CampusDetails Create(
        string? address = null,
        string? postalCode = null,
        string? phone = null,
        string? website = null) =>
        CampusDetails.Create(address, null, null, postalCode, phone, website, null, null, null);
}
