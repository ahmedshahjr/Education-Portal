using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AddOnRequests.Commands.RejectAddOnRequest;

/// <summary>Rejects a pending add-on request; nothing changes in the subscription. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RejectAddOnRequestCommand(Guid Id) : IRequest;

public class RejectAddOnRequestCommandValidator : AbstractValidator<RejectAddOnRequestCommand>
{
    public RejectAddOnRequestCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Id)
            .MustAsync(async (id, cancellationToken) => !await context.AddOnRequests.AnyAsync(r => r.PublicId == id && r.Status != AddOnRequestStatus.Pending, cancellationToken))
            .WithMessage(_ => ValidationMessages.AddOnRequestNotPending);
    }
}

public class RejectAddOnRequestCommandHandler : IRequestHandler<RejectAddOnRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public RejectAddOnRequestCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RejectAddOnRequestCommand request, CancellationToken cancellationToken)
    {
        var addOnRequest = await _context.AddOnRequests.FirstOrDefaultAsync(r => r.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, addOnRequest);

        addOnRequest.Reject(_timeProvider.GetUtcNow());
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AddOnRequestDecided,
            SecurityAuditOutcome.Success,
            targetType: nameof(AddOnRequest),
            targetId: addOnRequest.Id.ToString(CultureInfo.InvariantCulture));
    }
}
