using System.Globalization;
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
public record UploadSchoolPhotoCommand : IRequest<Guid>
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
            .WithMessage("The photo must be a PNG, JPEG or WebP image of at most 2 MB.");
    }
}

public class UploadSchoolPhotoCommandHandler : IRequestHandler<UploadSchoolPhotoCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UploadSchoolPhotoCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
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

        var photo = SchoolPhoto.Create(schoolId, campusId, request.Kind, request.Caption, request.SortOrder, request.Content);
        _context.SchoolPhotos.Add(photo);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolPhotoUploaded,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolPhoto),
            targetId: photo.Id.ToString(CultureInfo.InvariantCulture));

        return photo.PublicId;
    }
}
