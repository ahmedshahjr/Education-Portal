using EducationPortal.Domain.Entities;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Application.Branding.Common;

/// <summary>
/// The branding fields a school administrator (for their own school) or the master tenant (for any school) can change.
/// Images are raw bytes; their type and size are checked by <see cref="BrandingUpdateValidator{T}"/> and again by the
/// entity (SEC-INPUT-04).
/// </summary>
public abstract record BrandingUpdate
{
    /// <summary>The accent colour as <c>#rrggbb</c>.</summary>
    public string AccentColor { get; init; } = School.DefaultAccentColor.Value;

    /// <summary>The sidebar colour as <c>#rrggbb</c>, or <c>null</c> for the theme's default.</summary>
    public string? SidebarColor { get; init; }

    /// <summary>A new logo, or <c>null</c> to keep the current one.</summary>
    public byte[]? Logo { get; init; }

    /// <summary>Removes the logo, so the default one is shown.</summary>
    public bool RemoveLogo { get; init; }

    /// <summary>A new favicon, or <c>null</c> to keep the current one.</summary>
    public byte[]? Favicon { get; init; }

    /// <summary>Removes the favicon, so the default one is shown.</summary>
    public bool RemoveFavicon { get; init; }
}

/// <summary>The rules shared by both branding commands (SEC-INPUT-01).</summary>
public abstract class BrandingUpdateValidator<T> : AbstractValidator<T>
    where T : BrandingUpdate
{
    protected BrandingUpdateValidator()
    {
        RuleFor(u => u.AccentColor)
            .Must(HexColor.IsValid)
            .WithMessage("The accent colour must have the form #rrggbb.");

        RuleFor(u => u.SidebarColor)
            .Must(HexColor.IsValid)
            .When(u => u.SidebarColor is not null)
            .WithMessage("The sidebar colour must have the form #rrggbb.");

        RuleFor(u => u.Logo)
            .Must(logo => BrandingImageRules.AllowedContentType(BrandingImageKind.Logo, logo) is not null)
            .When(u => u.Logo is not null)
            .WithMessage($"The logo must be a PNG, JPEG or WebP image of at most {BrandingImageRules.MaxLogoBytes / 1024} KB.");

        RuleFor(u => u.Favicon)
            .Must(favicon => BrandingImageRules.AllowedContentType(BrandingImageKind.Favicon, favicon) is not null)
            .When(u => u.Favicon is not null)
            .WithMessage($"The favicon must be an ICO or PNG image of at most {BrandingImageRules.MaxFaviconBytes / 1024} KB.");

        RuleFor(u => u.RemoveLogo)
            .Equal(false)
            .When(u => u.Logo is not null)
            .WithMessage("Upload a new logo or remove it, not both.");

        RuleFor(u => u.RemoveFavicon)
            .Equal(false)
            .When(u => u.Favicon is not null)
            .WithMessage("Upload a new favicon or remove it, not both.");
    }
}
