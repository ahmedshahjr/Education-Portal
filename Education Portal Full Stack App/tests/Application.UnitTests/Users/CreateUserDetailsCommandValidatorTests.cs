using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Users.Commands.CreateUserDetails;
using EducationPortal.Domain.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users;

public class CreateUserDetailsCommandValidatorTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private CreateUserDetailsCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserDetailsCommandValidator(_identity.Object);
    }

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

    [Test]
    public async Task ShouldAcceptAPersonWithAnAccountWhenTheEmailAndPasswordAreValid()
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            Email = "bilal@school.local",
            OneTimePassword = "violet harbour lantern 47",
        });

        result.IsValid.ShouldBeTrue();
    }

    [TestCase("not-an-email")]
    [TestCase("bilal@")]
    public async Task ShouldRefuseAMalformedEmail(string email)
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            Email = email,
            OneTimePassword = "violet harbour lantern 47",
        });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.Email));
    }

    [Test]
    public async Task ShouldRefuseAnEmailAlreadyUsedInThePlatform()
    {
        _identity.Setup(i => i.IsEmailTakenAsync("bilal@school.local")).ReturnsAsync(true);

        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            Email = "bilal@school.local",
            OneTimePassword = "violet harbour lantern 47",
        });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.Email));
    }

    [TestCase(14, false)]
    [TestCase(15, true)]
    [TestCase(128, true)]
    [TestCase(129, false)]
    public async Task ShouldLimitTheOneTimePasswordTo15To128Characters(int length, bool valid)
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            Email = "bilal@school.local",
            OneTimePassword = new string('p', length),
        });

        result.IsValid.ShouldBe(valid);
    }

    [Test]
    public async Task ShouldRefuseAPasswordWithoutAnEmail()
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            OneTimePassword = "violet harbour lantern 47",
        });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.Email));
    }

    [Test]
    public async Task ShouldRefuseAnEmailWithoutAPassword()
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand
        {
            FirstName = "Bilal",
            LastName = "Ahmed",
            UserType = UserType.Teacher,
            Email = "bilal@school.local",
        });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserDetailsCommand.OneTimePassword));
    }

    [Test]
    public async Task ShouldAcceptAPersonWithoutAnAccount()
    {
        var result = await _validator.ValidateAsync(new CreateUserDetailsCommand { FirstName = "Ayesha", LastName = "Khan", UserType = UserType.Staff });

        result.IsValid.ShouldBeTrue();
    }
}
