namespace EducationPortal.Domain.Enums;

/// <summary>
/// The images a school can upload for its branding. Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member, because it is part of the image URL.
/// </summary>
public enum BrandingImageKind
{
    Logo = 1,
    Favicon = 2,
}
