using System.Security.Cryptography;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A photo in a school's public gallery (the table <c>SchoolPhotos</c>). The type is read from the file's first bytes, never
/// from its name or the content type the browser sent (SEC-INPUT-04), and only PNG, JPEG and WebP of at most
/// <see cref="MaxBytes"/> are kept. A deleted photo stays in the table (soft delete).
/// </summary>
public class SchoolPhoto : BaseSoftDeletableEntity, ITenantOwned
{
    public const int MaxBytes = 2 * 1024 * 1024;
    public const int CaptionMaxLength = 200;
    public const int VersionLength = 16;

    private SchoolPhoto()
    {
        // For EF Core.
        ContentType = string.Empty;
        Content = [];
        Version = string.Empty;
    }

    public static SchoolPhoto Create(int schoolId, int campusId, PhotoKind kind, string? caption, int sortOrder, byte[] content)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        var photo = new SchoolPhoto { SchoolId = schoolId, CampusId = campusId, Kind = kind, SortOrder = sortOrder };
        photo.Caption = CleanCaption(caption);
        photo.Replace(content);
        return photo;
    }

    public int SchoolId { get; private set; }

    /// <summary>The campus the photo shows, or <c>null</c> for the school as a whole.</summary>
    public int CampusId { get; private set; }

    /// <summary>
    /// The key that ties the copies of one whole-school item together, one copy per campus. It is <c>null</c> for an item
    /// that belongs to one campus. Changing or deleting a copy changes or deletes every copy with the same key, and a
    /// campus added later gets a copy of each key the school has (ADR-027).
    /// </summary>
    public Guid? WholeSchoolKey { get; private set; }

    /// <summary>Marks this row as one campus's copy of a whole-school item.</summary>
    public void JoinWholeSchool(Guid key)
    {
        if (key == Guid.Empty)
        {
            throw new ArgumentException("A whole-school key is not empty.", nameof(key));
        }

        WholeSchoolKey = key;
    }

    public PhotoKind Kind { get; private set; }

    public string? Caption { get; private set; }

    /// <summary>Where the photo appears in the gallery, lowest first.</summary>
    public int SortOrder { get; private set; }

    public string ContentType { get; private set; }

    public byte[] Content { get; private set; }

    /// <summary>Changes whenever the content changes, so clients can put it in the image URL to skip stale copies.</summary>
    public string Version { get; private set; }

    public void Describe(string? caption, int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        Caption = CleanCaption(caption);
        SortOrder = sortOrder;
    }

    public void Replace(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var detected = content.Length is > 0 and <= MaxBytes ? BrandingImageRules.DetectContentType(content) : null;
        if (detected is not (BrandingImageRules.Png or BrandingImageRules.Jpeg or BrandingImageRules.WebP))
        {
            throw new ArgumentException("The file is not a PNG, JPEG or WebP image of at most 2 MB.", nameof(content));
        }

        ContentType = detected;
        Content = content;
        Version = Convert.ToHexStringLower(SHA256.HashData(content))[..VersionLength];
    }

    private static string? CleanCaption(string? caption)
    {
        var trimmed = caption?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, CaptionMaxLength, nameof(caption));
        return trimmed;
    }
}
