using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Branding.Common;

/// <summary>
/// Reads and writes one school's branding. The caller decides which school: the current tenant, or a school chosen in
/// a master-tenant use case (SEC-AUTHZ-04, SEC-AUTHZ-07).
/// </summary>
internal static class BrandingStore
{
    public static async Task<BrandingDto> ReadAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken)
    {
        var school = await context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new { s.Name, s.IsMaster, s.ThemePreset, s.SidebarColor })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(schoolId.ToString(System.Globalization.CultureInfo.InvariantCulture), nameof(School));

        var images = await context.BrandingImages
            .AsNoTracking()
            .Where(i => i.SchoolId == schoolId)
            .Select(i => new { i.Kind, i.Version })
            .ToListAsync(cancellationToken);

        return new BrandingDto(
            school.Name,
            school.IsMaster,
            school.ThemePreset,
            school.SidebarColor?.Value,
            images.SingleOrDefault(i => i.Kind == BrandingImageKind.Logo)?.Version,
            images.SingleOrDefault(i => i.Kind == BrandingImageKind.Favicon)?.Version);
    }

    /// <summary>Applies <paramref name="update"/> to the school and saves it in one transaction (ENG-EF-01).</summary>
    public static async Task WriteAsync(IApplicationDbContext context, int schoolId, BrandingUpdate update, CancellationToken cancellationToken)
    {
        var school = await context.Schools.SingleOrDefaultAsync(s => s.Id == schoolId, cancellationToken)
            ?? throw new NotFoundException(schoolId.ToString(System.Globalization.CultureInfo.InvariantCulture), nameof(School));

        school.UpdateBranding(
            update.ThemePreset,
            update.SidebarColor is null ? null : HexColor.From(update.SidebarColor));

        await ApplyImageAsync(context, schoolId, BrandingImageKind.Logo, update.Logo, update.RemoveLogo, cancellationToken);
        await ApplyImageAsync(context, schoolId, BrandingImageKind.Favicon, update.Favicon, update.RemoveFavicon, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task ApplyImageAsync(
        IApplicationDbContext context,
        int schoolId,
        BrandingImageKind kind,
        byte[]? content,
        bool remove,
        CancellationToken cancellationToken)
    {
        if (content is null && !remove)
        {
            return;
        }

        var existing = await context.BrandingImages
            .SingleOrDefaultAsync(i => i.SchoolId == schoolId && i.Kind == kind, cancellationToken);

        if (remove)
        {
            if (existing is not null)
            {
                context.BrandingImages.Remove(existing);
            }

            return;
        }

        if (existing is null)
        {
            context.BrandingImages.Add(new BrandingImage(schoolId, kind, content!));
        }
        else
        {
            existing.Replace(content!);
        }
    }
}
