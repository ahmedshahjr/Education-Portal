using EducationPortal.Application.Users.Commands.CreateUserDetails;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users;

public class CreateUserDetailsCommandValidatorTests
{
    private readonly CreateUserDetailsCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptAPersonWithAMiddleName()
    {
        var result = _validator.Validate(new CreateUserDetailsCommand
        {
            FirstName = "Ayesha",
            MiddleName = "Noor",
            LastName = "Khan",
            UserType = UserType.Principal,
            Phone = "+92 300 1234567",
        });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptAPersonWithoutAMiddleName()
    {
        var result = _validator.Validate(new CreateUserDetailsCommand { FirstName = "Ayesha", LastName = "Khan", UserType = UserType.Teacher });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase(100, true)]
    [TestCase(101, false)]
    public void ShouldLimitTheMiddleNameTo100Characters(int length, bool valid)
    {
        var result = _validator.Validate(new CreateUserDetailsCommand
        {
            FirstName = "Ayesha",
            MiddleName = new string('m', length),
            LastName = "Khan",
            UserType = UserType.Staff,
        });

        result.IsValid.ShouldBe(valid);
    }

    [Test]
    public void ShouldRefuseAMissingLastName()
    {
        var result = _validator.Validate(new CreateUserDetailsCommand { FirstName = "Ayesha", UserType = UserType.Staff });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.LastName));
    }

    [Test]
    public void ShouldRefuseAUserTypeThatIsNotDefined()
    {
        var result = _validator.Validate(new CreateUserDetailsCommand { FirstName = "Ayesha", LastName = "Khan", UserType = (UserType)99 });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.UserType));
    }
}
