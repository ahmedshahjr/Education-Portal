using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Schools.Commands.DeleteSchool;

/// <summary>
/// Deletes a school (soft delete): the school disappears everywhere, its host is refused, and the lifecycle job no longer
/// runs its subscription. The row is kept for audit. The master tenant cannot be deleted. A master-tenant use case
/// (SEC-AUTHZ-04, SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record DeleteSchoolCommand(Guid SchoolId) : IRequest;

public class DeleteSchoolCommandHandler : IRequestHandler<DeleteSchoolCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IUser _user;

    public DeleteSchoolCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, IUser user)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _user = user;
    }

    public async Task Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        if (school.IsMaster)
        {
            throw new Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.SchoolId), ValidationMessages.SchoolMasterNotDeletable)]);
        }

        school.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
