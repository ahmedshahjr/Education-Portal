using EducationPortal.Application.Common.Security;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Identity;

/// <summary>
/// The password rules that Identity does not cover (SEC-AUTHN-03): the maximum length, common passwords, and words taken
/// from the product or the account. The minimum length is covered by Identity's own options.
/// </summary>
public class PasswordPolicyValidatorTests
{
    private const string Email = "jane.doe@example.com";

    private PasswordPolicyValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new PasswordPolicyValidator();
    }

    [Test]
    public async Task ShouldAcceptAPassphraseThatIsNotCommonOrPersonal()
    {
        var result = await Validate("quiet-harbour-lantern-owl");

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptAPasswordOfTheMaximumLength()
    {
        var password = "Ab1-" + new string('z', PasswordRules.MaximumLength - 4);

        var result = await Validate(password);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectAPasswordOneCharacterLongerThanTheMaximum()
    {
        var password = "Ab1-" + new string('z', PasswordRules.MaximumLength - 3);

        var result = await Validate(password);

        result.Errors.Select(error => error.Code).ShouldBe(new[] { "PasswordTooLong" });
    }

    [TestCase("passwordpassword")]
    [TestCase("PasswordPassword")]
    [TestCase("1234567890123456")]
    [TestCase("qwertyuiopasdfg")]
    public async Task ShouldRejectAPasswordOnTheCommonList(string password)
    {
        // Matching ignores case, so a capital letter does not make a common password acceptable.
        var result = await Validate(password);

        result.Errors.Select(error => error.Code).ShouldBe(new[] { "CommonPassword" });
    }

    [Test]
    public async Task ShouldRejectAPasswordThatContainsTheProductName()
    {
        var result = await Validate("EducationPortal-2026-spring");

        result.Errors.Select(error => error.Code).ShouldBe(new[] { "PersonalWord" });
    }

    [Test]
    public async Task ShouldRejectAPasswordThatContainsTheLocalPartOfTheAccountEmail()
    {
        var result = await Validate("jane.doe-remembers-the-lake");

        result.Errors.Select(error => error.Code).ShouldBe(new[] { "PersonalWord" });
    }

    [Test]
    public async Task ShouldIgnoreAShortLocalPartBecauseItWouldMatchTooManyPasswords()
    {
        // "al" appears in this passphrase, but a two-letter name is too short to count as personal.
        var result = await Validate("correct-al-horse-battery", "al@example.com");

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptAnEmptyPasswordAndLeaveItToIdentity()
    {
        var result = await Validate(string.Empty);

        result.Succeeded.ShouldBeTrue();
    }

    private Task<IdentityResult> Validate(string? password, string email = Email)
    {
        var user = new ApplicationUser { UserName = email, Email = email };

        // The validator does not use the manager, so none is passed.
        return _validator.ValidateAsync(null!, user, password);
    }
}
