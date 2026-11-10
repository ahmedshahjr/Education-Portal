using EducationPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The multipart form for changing a school's branding. Files are read into memory: the request body is capped at 1 MB
/// by Kestrel (SEC-WEB-07), and the validator then checks each file's real type and size (SEC-INPUT-04).
/// </summary>
public sealed class BrandingForm
{
    public ThemePreset ThemePreset { get; init; }

    /// <summary>The sidebar colour as #rrggbb. Leave it out for the theme's default.</summary>
    public string? SidebarColor { get; init; }

    /// <summary>A PNG, JPEG or WebP logo of at most 512 KB.</summary>
    public IFormFile? Logo { get; init; }

    public bool RemoveLogo { get; init; }

    /// <summary>An ICO or PNG favicon of at most 100 KB.</summary>
    public IFormFile? Favicon { get; init; }

    public bool RemoveFavicon { get; init; }

    internal static async Task<byte[]?> ReadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        using var buffer = new MemoryStream((int)Math.Min(file.Length, int.MaxValue));
        await file.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
