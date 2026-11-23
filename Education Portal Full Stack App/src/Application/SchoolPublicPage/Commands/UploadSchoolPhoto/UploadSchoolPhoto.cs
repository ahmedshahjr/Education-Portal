using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.UploadSchoolPhoto;

/// <summary>
/// Adds a photo to a school's gallery. The image is at most 2 MB and must be a PNG, JPEG or WebP by its bytes. The campus,
/// when given, is the school's own. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UploadSchoolPhotoCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The campus the photo shows, or <c>null</c> for the school as a whole.</summary>
    public Guid? CampusId { get; init; }

    public PhotoKind Kind { get; init; }

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    public byte[] Content { get; init; } = [];
}

public class UploadSchoolPhotoCommandValidator : AbstractValidator<UploadSchoolPhotoCommand>
{
    public UploadSchoolPhotoCommandValidator()
    {
        RuleFor(c => c.Kind).IsInEnum();

        RuleFor(c => c.Caption)
            .MaximumLength(SchoolPhoto.CaptionMaxLength);

        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);

        RuleFor(c => c.CampusId)
            .NotEqual(Guid.Empty)
            .When(c => c.CampusId.HasValue);

        RuleFor(c => c.Content)
            .Must(SchoolPhotoRules.IsAcceptable)
            .WithMessage(_ => ValidationMessages.PhotoNotAcceptable);
    }
}

public class UploadSchoolPhotoCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly ICampusAccess _campusAccess;

    public UploadSchoolPhotoCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(UploadSchoolPhotoCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        int? campusId = request.CampusId is { } campusPublicId
            ? await _context.Campuses.Where(c => c.SchoolId == schoolId).ResolveIdAsync(campusPublicId, cancellationToken)
            : null;
        var campusIds = await WholeSchoolCopies.CampusIdsAsync(_context, schoolId, campusId, cancellationToken);
        foreach (var id in campusIds)
        {
            await _campusAccess.EnsureCampusAsync(id, ModuleCode.School, AccessRight.Create, cancellationToken);
        }

        // A whole-school photo is one row per campus, all with the same key; the bytes are copied to each campus.
        var key = campusId is null ? Guid.CreateVersion7() : (Guid?)null;
        var photos = campusIds.Select(id =>
        {
            var photo = SchoolPhoto.Create(schoolId, id, request.Kind, request.Caption, request.SortOrder, request.Content);
            if (key is { } groupKey)
            {
                photo.JoinWholeSchool(groupKey);
            }

            return photo;
        }).ToList();

        _context.SchoolPhotos.AddRange(photos);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolPhotoUploaded,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolPhoto),
            targetId: photos[0].Id.ToString(CultureInfo.InvariantCulture));

        return photos[0].PublicId;
    }
}
