using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Campuses.Commands.DeleteCampus;

/// <summary>
/// Deletes a campus of the current school. The row is kept (soft delete) and the campus disappears everywhere. The main campus
/// cannot be deleted, and a campus that still has module grants must have them turned off first. Needs the Campuses module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Campus)]
public record DeleteCampusCommand : IRequest
{
    /// <summary>The campus's public id, taken from the route (ENG-ID-02).</summary>
    public Guid CampusId { get; init; }
}

public class DeleteCampusCommandHandler : IRequestHandler<DeleteCampusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;

    public DeleteCampusCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, ICurrentTenant tenant, IUser user)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _tenant = tenant;
        _user = user;
    }

    public async Task Handle(DeleteCampusCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var campus = await _context.Campuses.FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.CampusId, cancellationToken);
        Guard.Against.NotFound(request.CampusId, campus);

        if (campus.Kind == CampusKind.Main)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.CampusId), ValidationMessages.CampusMainNotDeletable)]);
        }

        var hasModules = await _context.TenantModules.AnyAsync(
            t => t.CampusId == campus.Id && t.Status != TenantModuleStatus.Expired, cancellationToken);
        if (hasModules)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.CampusId), ValidationMessages.CampusHasModules)]);
        }

        campus.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CampusDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(Campus),
            targetId: campus.Id.ToString(CultureInfo.InvariantCulture));
    }
}
