using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AddOnRequests.Commands.RequestAddOn;

/// <summary>
/// The current school's administrator asks for a paid module to be added to the school's subscription, and the new request's
/// id is returned. Nothing changes until the master tenant approves it. The school comes from the host, never from the
/// request (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.School)]
public record RequestAddOnCommand : IRequest<Guid>
{
    public string ModuleCode { get; init; } = string.Empty;
}

public class RequestAddOnCommandValidator : AbstractValidator<RequestAddOnCommand>
{
    public RequestAddOnCommandValidator(IApplicationDbContext context, ICurrentTenant tenant)
    {
        RuleFor(c => c.ModuleCode).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);

        RuleFor(c => c.ModuleCode)
            .MustAsync(async (code, cancellationToken) => await context.Modules.AnyAsync(m => m.Code == code && m.IsActive && !m.IsCore, cancellationToken))
            .WithMessage(_ => ValidationMessages.UnknownModules)
            .MustAsync(async (code, cancellationToken) => tenant.SchoolId is { } schoolId && await context.Subscriptions.AnyAsync(s => s.SchoolId == schoolId && s.Status != SubscriptionStatus.Cancelled, cancellationToken))
            .WithMessage(_ => ValidationMessages.SubscriptionNeededForAddOn)
            .MustAsync(async (code, cancellationToken) => !await (
                    from t in context.TenantModules
                    join m in context.Modules on t.ModuleId equals m.Id
                    where t.SchoolId == tenant.SchoolId && m.Code == code && t.BranchId == null && t.Status != TenantModuleStatus.Expired
                    select t.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.ModuleAlreadyInSubscription)
            .MustAsync(async (code, cancellationToken) => !await (
                    from r in context.AddOnRequests
                    join m in context.Modules on r.ModuleId equals m.Id
                    where r.SchoolId == tenant.SchoolId && m.Code == code && r.Status == AddOnRequestStatus.Pending
                    select r.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.AddOnRequestPending);
    }
}

public class RequestAddOnCommandHandler : IRequestHandler<RequestAddOnCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly ISecurityAuditLog _auditLog;

    public RequestAddOnCommandHandler(IApplicationDbContext context, ICurrentTenant tenant, ISecurityAuditLog auditLog)
    {
        _context = context;
        _tenant = tenant;
        _auditLog = auditLog;
    }

    public async Task<Guid> Handle(RequestAddOnCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var module = await _context.Modules.FirstAsync(m => m.Code == request.ModuleCode, cancellationToken);
        var addOnRequest = AddOnRequest.Create(schoolId, module);
        _context.AddOnRequests.Add(addOnRequest);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AddOnRequested,
            SecurityAuditOutcome.Success,
            targetType: nameof(AddOnRequest),
            targetId: addOnRequest.Id.ToString(CultureInfo.InvariantCulture));

        return addOnRequest.PublicId;
    }
}
