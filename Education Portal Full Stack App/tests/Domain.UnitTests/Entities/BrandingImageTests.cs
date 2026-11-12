using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class BrandingImageTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00];
    private static readonly byte[] WebP = [.. "RIFF"u8, 0, 0, 0, 0, .. "WEBP"u8, 0];
    private static readonly byte[] Icon = [0x00, 0x00, 0x01, 0x00, 0x01, 0x00];
    private static readonly byte[] Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"/>"u8.ToArray();

    [Test]
    public void ShouldDetectAllowedFormatsByTheirSignature()
    {
        BrandingImageRules.DetectContentType(Png).ShouldBe("image/png");
        BrandingImageRules.DetectContentType(Jpeg).ShouldBe("image/jpeg");
        BrandingImageRules.DetectContentType(WebP).ShouldBe("image/webp");
        BrandingImageRules.DetectContentType(Icon).ShouldBe("image/x-icon");
    }

    [Test]
    public void ShouldNotRecogniseSvgOrText()
    {
        BrandingImageRules.DetectContentType(Svg).ShouldBeNull();
        BrandingImageRules.DetectContentType("GIF89a"u8).ShouldBeNull();
        BrandingImageRules.DetectContentType([]).ShouldBeNull();
    }

    [Test]
    public void ShouldAllowDifferentFormatsForLogosAndFavicons()
    {
        BrandingImageRules.AllowedContentType(BrandingImageKind.Logo, Jpeg).ShouldBe("image/jpeg");
        BrandingImageRules.AllowedContentType(BrandingImageKind.Logo, Icon).ShouldBeNull();
        BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, Icon).ShouldBe("image/x-icon");
        BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, Jpeg).ShouldBeNull();
    }

    [Test]
    public void ShouldRefuseAFileOverTheSizeLimit()
    {
        byte[] large = [.. Png, .. new byte[BrandingImageRules.MaxFaviconBytes]];

        BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, large).ShouldBeNull();
        BrandingImageRules.AllowedContentType(BrandingImageKind.Logo, large).ShouldBe("image/png");
    }

    [Test]
    public void ShouldAcceptAFileExactlyAtTheSizeLimit()
    {
        byte[] atLimit = [.. Png, .. new byte[BrandingImageRules.MaxFaviconBytes - Png.Length]];

        atLimit.Length.ShouldBe(BrandingImageRules.MaxFaviconBytes);
        BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, atLimit).ShouldBe("image/png");
    }

    [Test]
    public void ShouldRefuseAnEmptyFile()
    {
        BrandingImageRules.AllowedContentType(BrandingImageKind.Logo, []).ShouldBeNull();
        BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, []).ShouldBeNull();
    }

    [Test]
    public void ShouldStoreTheDetectedTypeAndAVersion()
    {
        var image = new BrandingImage(1, BrandingImageKind.Logo, Png);

        image.ContentType.ShouldBe("image/png");
        image.Version.Length.ShouldBe(BrandingImage.VersionLength);
    }

    [Test]
    public void ShouldChangeTheVersionWhenTheContentChanges()
    {
        var image = new BrandingImage(1, BrandingImageKind.Logo, Png);
        var before = image.Version;

        image.Replace(Jpeg);

        image.Version.ShouldNotBe(before);
        image.ContentType.ShouldBe("image/jpeg");
    }

    [Test]
    public void ShouldRefuseContentThatIsNotAllowed()
    {
        Should.Throw<ArgumentException>(() => new BrandingImage(1, BrandingImageKind.Logo, Svg));
    }
}
