using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Branding.Commands.UpdateBranding;

/// <summary>
/// Changes the branding of the administrator's own school, the one that owns the request's domain (SEC-AUTHZ-04).
/// Administrators only, until roles with rights per module arrive in step 2b. The change is audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.School)]
public record UpdateBrandingCommand : BrandingUpdate, IRequest;

public class UpdateBrandingCommandValidator : BrandingUpdateValidator<UpdateBrandingCommand>;

public class UpdateBrandingCommandHandler : IRequestHandler<UpdateBrandingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateBrandingCommandHandler(IApplicationDbContext context, ICurrentTenant tenant, ISecurityAuditLog auditLog)
    {
        _context = context;
        _tenant = tenant;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new UnauthorizedAccessException();

        await BrandingStore.WriteAsync(_context, schoolId, request, cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BrandingChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: schoolId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
