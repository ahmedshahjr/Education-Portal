using EducationPortal.Application.Branding.Commands.UpdateBranding;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Branding;

public class UpdateBrandingCommandValidatorTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    private readonly UpdateBrandingCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptAWellFormedCommand()
    {
        var result = _validator.Validate(new UpdateBrandingCommand
        {
            AccentColor = "#0074ba",
            SidebarColor = "#123abc",
            Logo = Png,
            Favicon = Png,
        });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAnAccentColourThatIsNotHex()
    {
        var result = _validator.Validate(new UpdateBrandingCommand { AccentColor = "orange" });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateBrandingCommand.AccentColor));
    }

    [Test]
    public void ShouldRefuseAColourThatIsNotHex()
    {
        var result = _validator.Validate(new UpdateBrandingCommand { AccentColor = "#00a1ff", SidebarColor = "blue" });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateBrandingCommand.SidebarColor));
    }

    [Test]
    public void ShouldRefuseALogoThatIsNotAnAllowedImage()
    {
        var result = _validator.Validate(new UpdateBrandingCommand { AccentColor = "#00a1ff", Logo = "<svg/>"u8.ToArray() });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateBrandingCommand.Logo));
    }

    [Test]
    public void ShouldRefuseUploadingAndRemovingTheSameImage()
    {
        var result = _validator.Validate(new UpdateBrandingCommand { AccentColor = "#00a1ff", Logo = Png, RemoveLogo = true });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateBrandingCommand.RemoveLogo));
    }
}
