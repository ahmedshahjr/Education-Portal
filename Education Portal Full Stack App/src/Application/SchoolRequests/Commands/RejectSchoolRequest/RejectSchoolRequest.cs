using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.SchoolRequests.Commands.RejectSchoolRequest;

/// <summary>
/// The master tenant rejects a pending school request. A rejected request creates no school and no account. Master tenant
/// administrators only (SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RejectSchoolRequestCommand
{
    /// <summary>The request's public id, taken from the route (ENG-ID-02).</summary>
    public Guid RequestId { get; init; }
}

public class RejectSchoolRequestCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public RejectSchoolRequestCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(RejectSchoolRequestCommand request, CancellationToken cancellationToken)
    {
        var schoolRequest = await _context.SchoolRequests.FirstOrDefaultAsync(r => r.PublicId == request.RequestId, cancellationToken);
        Guard.Against.NotFound(request.RequestId, schoolRequest);

        if (schoolRequest.Status != SchoolRequestStatus.Pending)
        {
            throw new Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.RequestId), ValidationMessages.SchoolRequestNotPending)]);
        }

        schoolRequest.Reject();
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolRequestRejected,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRequest),
            targetId: schoolRequest.Id.ToString(CultureInfo.InvariantCulture));
    }
}
