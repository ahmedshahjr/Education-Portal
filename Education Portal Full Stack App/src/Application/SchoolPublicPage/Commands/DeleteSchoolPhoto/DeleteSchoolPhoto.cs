using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolPublicPage.Commands.DeleteSchoolPhoto;

/// <summary>Deletes a gallery photo. The row is kept (soft delete) and hidden everywhere. Needs the School module. Audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record DeleteSchoolPhotoCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The photo's public id, taken from the route.</summary>
    public Guid PhotoId { get; init; }
}

public class DeleteSchoolPhotoCommandValidator : AbstractValidator<DeleteSchoolPhotoCommand>
{
    public DeleteSchoolPhotoCommandValidator()
    {
        RuleFor(c => c.PhotoId).NotEmpty();
    }
}

public class DeleteSchoolPhotoCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteSchoolPhotoCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteSchoolPhotoCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var photo = await _context.SchoolPhotos.FirstOrDefaultAsync(p => p.SchoolId == schoolId && p.PublicId == request.PhotoId, cancellationToken);
        Guard.Against.NotFound(request.PhotoId, photo);

        // A whole-school photo is deleted for every campus at once.
        var rows = photo.WholeSchoolKey is { } key
            ? await _context.SchoolPhotos.Where(p => p.SchoolId == schoolId && p.WholeSchoolKey == key).ToListAsync(cancellationToken)
            : [photo];

        var now = _timeProvider.GetUtcNow();
        foreach (var row in rows)
        {
            row.Delete(now, _user.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolPhotoDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolPhoto),
            targetId: photo.Id.ToString(CultureInfo.InvariantCulture));
    }
}
