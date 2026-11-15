using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class UserDetailsTests
{
    [Test]
    public void ShouldTrimTheNamesAndKeepTheMiddleNameWhenGiven()
    {
        var person = UserDetails.Create(1, "  Ayesha ", "  Noor  ", " Khan ", UserType.Principal, null);

        person.FirstName.ShouldBe("Ayesha");
        person.MiddleName.ShouldBe("Noor");
        person.LastName.ShouldBe("Khan");
        person.UserType.ShouldBe(UserType.Principal);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldStoreAMissingMiddleNameAsNull(string? middleName)
    {
        UserDetails.Create(1, "Ayesha", middleName, "Khan", UserType.Teacher, null).MiddleName.ShouldBeNull();
    }

    [TestCase(UserDetails.NameMaxLength, true)]
    [TestCase(UserDetails.NameMaxLength + 1, false)]
    public void ShouldLimitTheMiddleNameTo100Characters(int length, bool accepted)
    {
        Action create = () => UserDetails.Create(1, "Ayesha", new string('m', length), "Khan", UserType.Staff, null);

        if (accepted)
        {
            create();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(create);
        }
    }

    [TestCase(UserDetails.NameMaxLength, true)]
    [TestCase(UserDetails.NameMaxLength + 1, false)]
    public void ShouldLimitTheFirstAndLastNameTo100Characters(int length, bool accepted)
    {
        Action first = () => UserDetails.Create(1, new string('f', length), null, "Khan", UserType.Staff, null);
        Action last = () => UserDetails.Create(1, "Ayesha", null, new string('l', length), UserType.Staff, null);

        if (accepted)
        {
            first();
            last();
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(first);
            Should.Throw<ArgumentOutOfRangeException>(last);
        }
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseAMissingFirstOrLastName(string name)
    {
        Should.Throw<ArgumentException>(() => UserDetails.Create(1, name, null, "Khan", UserType.Staff, null));
        Should.Throw<ArgumentException>(() => UserDetails.Create(1, "Ayesha", null, name, UserType.Staff, null));
    }

    [Test]
    public void ShouldRefuseAUserTypeThatIsNotDefined()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => UserDetails.Create(1, "Ayesha", null, "Khan", (UserType)99, null));
    }

    [Test]
    public void ShouldRefuseAPhoneThatIsNotValid()
    {
        Should.Throw<ArgumentException>(() => UserDetails.Create(1, "Ayesha", null, "Khan", UserType.Teacher, "123"));
    }

    [Test]
    public void ShouldRefuseAPersonWithoutASchool()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => UserDetails.Create(0, "Ayesha", null, "Khan", UserType.Teacher, null));
    }
}
