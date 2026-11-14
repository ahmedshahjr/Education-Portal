using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Commands.ChangePassword;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users.Commands.ChangePassword;

public class ChangePasswordCommandValidatorTests
{
    private const string Current = "violet harbour lantern 47";

    private readonly ChangePasswordCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptANewPasswordOfTheMinimumLength()
    {
        var command = new ChangePasswordCommand { CurrentPassword = Current, NewPassword = new string('q', PasswordRules.MinimumLength) };

        _validator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptANewPasswordOfTheMaximumLength()
    {
        var command = new ChangePasswordCommand { CurrentPassword = Current, NewPassword = new string('q', PasswordRules.MaximumLength) };

        _validator.Validate(command).IsValid.ShouldBeTrue();
    }

    [TestCase(0)]
    [TestCase(PasswordRules.MinimumLength - 1)]
    [TestCase(PasswordRules.MaximumLength + 1)]
    public void ShouldRefuseANewPasswordOfTheWrongLength(int length)
    {
        var command = new ChangePasswordCommand { CurrentPassword = Current, NewPassword = new string('q', length) };

        _validator.Validate(command).Errors.ShouldContain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Test]
    public void ShouldRefuseTheCurrentPasswordAsTheNewOne()
    {
        var command = new ChangePasswordCommand { CurrentPassword = Current, NewPassword = Current };

        _validator.Validate(command).Errors.ShouldContain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Test]
    public void ShouldRequireTheCurrentPassword()
    {
        var command = new ChangePasswordCommand { CurrentPassword = "", NewPassword = "quiet orchard ladder 93" };

        _validator.Validate(command).Errors.ShouldContain(e => e.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
    }
}
