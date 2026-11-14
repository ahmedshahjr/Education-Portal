namespace EducationPortal.Application.Branding.Common;

/// <summary>
/// A school's branding, as the UI needs it to theme itself. It is public (shown before sign-in), so it holds only what
/// the login page shows anyway: no ids, people or settings beyond the look. The versions change with each upload and
/// go into the image URLs; <c>null</c> means the school uses the default image.
/// </summary>
public sealed record BrandingDto(
    string SchoolName,
    bool IsMaster,
    string AccentColor,
    string? SidebarColor,
    string? LogoVersion,
    string? FaviconVersion);
