using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Commands.SignIn;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users.Commands.SignIn;

public class SignInCommandValidatorTests
{
    private const string AnyPassword = "a passphrase that is long enough";

    private SignInCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new SignInCommandValidator();
    }

    [TestCase("jane@example.com")]
    [TestCase("jane")]
    [TestCase("jane.doe-01")]
    public void ShouldAcceptAWellFormedCommand(string userName)
    {
        var result = _validator.Validate(new SignInCommand { UserName = userName, Password = AnyPassword });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectAnEmptyUserName()
    {
        var result = _validator.Validate(new SignInCommand { UserName = string.Empty, Password = AnyPassword });

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.UserName));
    }

    [TestCase("jane\r\nBcc: someone")]
    [TestCase("jane\tdoe")]
    [TestCase("jane\0")]
    public void ShouldRejectAUserNameWithControlCharacters(string userName)
    {
        var result = _validator.Validate(new SignInCommand { UserName = userName, Password = AnyPassword });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.UserName));
    }

    [Test]
    public void ShouldAcceptAUserNameOf256Characters()
    {
        var userName = new string('a', 256);

        var result = _validator.Validate(new SignInCommand { UserName = userName, Password = AnyPassword });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectAUserNameOf257Characters()
    {
        var userName = new string('a', 257);

        var result = _validator.Validate(new SignInCommand { UserName = userName, Password = AnyPassword });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.UserName));
    }

    [Test]
    public void ShouldRejectAnEmptyPassword()
    {
        var result = _validator.Validate(new SignInCommand { UserName = "jane", Password = string.Empty });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Password));
    }

    [Test]
    public void ShouldAcceptAPasswordOfTheMaximumLength()
    {
        var password = new string('x', PasswordRules.MaximumLength);

        var result = _validator.Validate(new SignInCommand { UserName = "jane", Password = password });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectAPasswordOneCharacterLongerThanTheMaximum()
    {
        var password = new string('x', PasswordRules.MaximumLength + 1);

        var result = _validator.Validate(new SignInCommand { UserName = "jane", Password = password });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Password));
    }
}
