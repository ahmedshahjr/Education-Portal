using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolPublicPage.Common;

/// <summary>
/// The upload rule for gallery photos (SEC-INPUT-04): at most <see cref="SchoolPhoto.MaxBytes"/>, and a PNG, JPEG or WebP
/// by its first bytes. The browser's file name and content type are never trusted.
/// </summary>
public static class SchoolPhotoRules
{
    public static bool IsAcceptable(byte[]? content) =>
        content is { Length: > 0 and <= SchoolPhoto.MaxBytes }
        && BrandingImageRules.DetectContentType(content) is BrandingImageRules.Png or BrandingImageRules.Jpeg or BrandingImageRules.WebP;
}
