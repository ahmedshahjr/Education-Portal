using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The multipart form of a gallery photo. The file is read into memory; the request size limit (2 MB plus the form's other
/// fields) stops larger bodies before they are read, and the validator checks the real type and size.
/// </summary>
public sealed class SchoolPhotoForm
{
    /// <summary>Room for the other form fields on top of the image.</summary>
    public const int EnvelopeAllowance = 64 * 1024;

    public Guid? CampusId { get; init; }

    public PhotoKind Kind { get; init; }

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    public IFormFile? File { get; init; }

    internal static async Task<byte[]?> ReadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        // Read at most one byte over the cap, so an oversized file is refused by the validator without being held in full.
        await using var stream = file.OpenReadStream();
        var limited = new byte[SchoolPhoto.MaxBytes + 1];
        var total = 0;
        int read;
        while ((read = await stream.ReadAsync(limited.AsMemory(total, limited.Length - total), cancellationToken)) > 0)
        {
            total += read;
            if (total == limited.Length)
            {
                break;
            }
        }

        return limited.AsSpan(0, total).ToArray();
    }
}
