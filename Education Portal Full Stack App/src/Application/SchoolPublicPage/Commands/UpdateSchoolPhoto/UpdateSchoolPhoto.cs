using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolPhoto;

/// <summary>
/// Changes a gallery photo's caption and order, and optionally replaces its image (same rules as an upload). The campus and
/// kind of a photo do not change: delete it and upload again. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UpdateSchoolPhotoCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The photo's public id, taken from the route.</summary>
    public Guid PhotoId { get; init; }

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    /// <summary>A new image, or <c>null</c> to keep the current one.</summary>
    public byte[]? Content { get; init; }
}

public class UpdateSchoolPhotoCommandValidator : AbstractValidator<UpdateSchoolPhotoCommand>
{
    public UpdateSchoolPhotoCommandValidator()
    {
        RuleFor(c => c.PhotoId).NotEmpty();

        RuleFor(c => c.Caption)
            .MaximumLength(SchoolPhoto.CaptionMaxLength);

        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);

        RuleFor(c => c.Content)
            .Must(SchoolPhotoRules.IsAcceptable)
            .WithMessage(_ => ValidationMessages.PhotoNotAcceptable)
            .When(c => c.Content is not null);
    }
}

public class UpdateSchoolPhotoCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateSchoolPhotoCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateSchoolPhotoCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var photo = await _context.SchoolPhotos.FirstOrDefaultAsync(p => p.SchoolId == schoolId && p.PublicId == request.PhotoId, cancellationToken);
        Guard.Against.NotFound(request.PhotoId, photo);

        // A whole-school photo is one row per campus: every copy changes together.
        var rows = photo.WholeSchoolKey is { } key
            ? await _context.SchoolPhotos.Where(p => p.SchoolId == schoolId && p.WholeSchoolKey == key).ToListAsync(cancellationToken)
            : [photo];

        foreach (var row in rows)
        {
            row.Describe(request.Caption, request.SortOrder);
            if (request.Content is not null)
            {
                row.Replace(request.Content);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolPhotoChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolPhoto),
            targetId: photo.Id.ToString(CultureInfo.InvariantCulture));
    }
}
