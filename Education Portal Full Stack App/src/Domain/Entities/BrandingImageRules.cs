namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a branding image may be (SEC-INPUT-04). The type is read from the file's first bytes, never from its name or
/// the content type the browser sent. Only raster formats are allowed: SVG can carry scripts, so it is refused.
/// </summary>
public static class BrandingImageRules
{
    public const int MaxLogoBytes = 512 * 1024;
    public const int MaxFaviconBytes = 100 * 1024;

    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
    public const string WebP = "image/webp";
    public const string Icon = "image/x-icon";

    public static int MaxBytes(BrandingImageKind kind) => kind switch
    {
        BrandingImageKind.Logo => MaxLogoBytes,
        BrandingImageKind.Favicon => MaxFaviconBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown branding image kind."),
    };

    public static IReadOnlyList<string> AllowedContentTypes(BrandingImageKind kind) => kind switch
    {
        BrandingImageKind.Logo => [Png, Jpeg, WebP],
        BrandingImageKind.Favicon => [Png, Icon],
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown branding image kind."),
    };

    /// <summary>
    /// Returns the content type the bytes are allowed as for <paramref name="kind"/>, or <c>null</c> when the file is
    /// empty, too large, or not one of the allowed formats.
    /// </summary>
    public static string? AllowedContentType(BrandingImageKind kind, ReadOnlySpan<byte> content)
    {
        if (content.IsEmpty || content.Length > MaxBytes(kind))
        {
            return null;
        }

        var detected = DetectContentType(content);

        return detected is not null && AllowedContentTypes(kind).Contains(detected) ? detected : null;
    }

    /// <summary>Recognises PNG, JPEG, WebP and ICO by their signatures. Returns <c>null</c> for anything else.</summary>
    public static string? DetectContentType(ReadOnlySpan<byte> content)
    {
        ReadOnlySpan<byte> png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        ReadOnlySpan<byte> jpeg = [0xFF, 0xD8, 0xFF];
        ReadOnlySpan<byte> riff = "RIFF"u8;
        ReadOnlySpan<byte> webp = "WEBP"u8;
        ReadOnlySpan<byte> icon = [0x00, 0x00, 0x01, 0x00];

        if (content.StartsWith(png))
        {
            return Png;
        }

        if (content.StartsWith(jpeg))
        {
            return Jpeg;
        }

        if (content.Length >= 12 && content.StartsWith(riff) && content[8..12].SequenceEqual(webp))
        {
            return WebP;
        }

        // An ICO header is followed by a non-zero image count.
        if (content.Length >= 6 && content.StartsWith(icon) && (content[4] | content[5]) != 0)
        {
            return Icon;
        }

        return null;
    }
}
