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

    [Test]
    public void ShouldAcceptAWellFormedCommand()
    {
        var result = _validator.Validate(new SignInCommand { Email = "jane@example.com", Password = AnyPassword });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase("not-an-email")]
    [TestCase("@example.com")]
    [TestCase("jane@")]
    [TestCase("two@@example.com")]
    [TestCase("jane@example.com\r\nBcc: someone@example.com")]
    public void ShouldRejectAMissingOrMalformedEmail(string email)
    {
        var result = _validator.Validate(new SignInCommand { Email = email, Password = AnyPassword });

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Email));
    }

    [Test]
    public void ShouldRejectAnEmailLongerThan256Characters()
    {
        var email = new string('a', 250) + "@example.com";

        var result = _validator.Validate(new SignInCommand { Email = email, Password = AnyPassword });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Email));
    }

    [Test]
    public void ShouldRejectAnEmptyPassword()
    {
        var result = _validator.Validate(new SignInCommand { Email = "jane@example.com", Password = string.Empty });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Password));
    }

    [Test]
    public void ShouldAcceptAPasswordOfTheMaximumLength()
    {
        var password = new string('x', PasswordRules.MaximumLength);

        var result = _validator.Validate(new SignInCommand { Email = "jane@example.com", Password = password });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectAPasswordOneCharacterLongerThanTheMaximum()
    {
        var password = new string('x', PasswordRules.MaximumLength + 1);

        var result = _validator.Validate(new SignInCommand { Email = "jane@example.com", Password = password });

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(SignInCommand.Password));
    }
}
