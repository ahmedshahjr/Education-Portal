using EducationPortal.Application.Users.Commands.SetAccountRoles;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users;

public class SetAccountRolesCommandValidatorTests
{
    private readonly SetAccountRolesCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptDistinctRoles()
    {
        var result = _validator.Validate(new SetAccountRolesCommand { PersonId = Guid.NewGuid(), RoleIds = [Guid.NewGuid(), Guid.NewGuid()] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptNoRolesAtAll()
    {
        var result = _validator.Validate(new SetAccountRolesCommand { PersonId = Guid.NewGuid(), RoleIds = [] });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseARoleGivenTwice()
    {
        var id = Guid.NewGuid();

        var result = _validator.Validate(new SetAccountRolesCommand { PersonId = Guid.NewGuid(), RoleIds = [id, id] });

        result.IsValid.ShouldBeFalse();
    }

    [TestCase(50, true)]
    [TestCase(51, false)]
    public void ShouldLimitTheRolesTo50(int count, bool valid)
    {
        var roles = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

        var result = _validator.Validate(new SetAccountRolesCommand { PersonId = Guid.NewGuid(), RoleIds = roles });

        result.IsValid.ShouldBe(valid);
    }
}
