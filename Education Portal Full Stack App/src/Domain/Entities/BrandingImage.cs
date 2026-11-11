using System.Security.Cryptography;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school's logo or favicon. Owned by one school (<see cref="SchoolId"/>), at most one per kind. The bytes are checked
/// by <see cref="BrandingImageRules"/> before they are stored, and the stored content type comes from that check.
/// </summary>
public class BrandingImage : BaseAuditableEntity
{
    public const int ContentTypeMaxLength = 32;
    public const int VersionLength = 16;

    private BrandingImage()
    {
        // For EF Core.
        ContentType = string.Empty;
        Content = [];
        Version = string.Empty;
    }

    public BrandingImage(long schoolId, BrandingImageKind kind, byte[] content)
        : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown branding image kind.");
        }

        SchoolId = schoolId;
        Kind = kind;
        Replace(content);
    }

    public long SchoolId { get; private set; }

    public BrandingImageKind Kind { get; private set; }

    /// <summary>The detected type, one of <see cref="BrandingImageRules.AllowedContentTypes"/>.</summary>
    public string ContentType { get; private set; }

    public byte[] Content { get; private set; }

    /// <summary>Changes whenever the content changes, so clients can put it in the image URL to skip stale copies.</summary>
    public string Version { get; private set; }

    public void Replace(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        ContentType = BrandingImageRules.AllowedContentType(Kind, content)
            ?? throw new ArgumentException("The file is not an allowed image or is too large.", nameof(content));
        Content = content;
        Version = Convert.ToHexStringLower(SHA256.HashData(content))[..VersionLength];
    }
}
