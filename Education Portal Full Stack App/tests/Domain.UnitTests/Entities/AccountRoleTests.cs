using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A role is given to an account of a school: the account id is present and fits the identity column.</summary>
public class AccountRoleTests
{
    [Test]
    public void ShouldGiveARoleToAnAccount()
    {
        var grant = AccountRole.Create(1, "user-id", 7);

        grant.SchoolId.ShouldBe(1);
        grant.UserId.ShouldBe("user-id");
        grant.SchoolRoleId.ShouldBe(7);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankAccountId(string userId)
    {
        Should.Throw<ArgumentException>(() => AccountRole.Create(1, userId, 7));
    }

    [TestCase(450, true)]
    [TestCase(451, false)]
    public void ShouldLimitTheAccountIdTo450Characters(int length, bool valid)
    {
        var userId = new string('u', length);

        if (valid)
        {
            AccountRole.Create(1, userId, 7).UserId.Length.ShouldBe(length);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(() => AccountRole.Create(1, userId, 7));
        }
    }

    [TestCase(0, 7)]
    [TestCase(1, 0)]
    public void ShouldRefuseAnIdThatIsNotASavedRow(int schoolId, int roleId)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AccountRole.Create(schoolId, "user-id", roleId));
    }
}
