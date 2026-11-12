using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Commands.UpdateSchoolBranding;

/// <summary>
/// Changes any school's branding from the master portal. A separate master-tenant use case, so the school id may come
/// from the route here and nowhere else (SEC-AUTHZ-04, SEC-AUTHZ-07). The change is audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateSchoolBrandingCommand : BrandingUpdate, IRequest
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }
}

public class UpdateSchoolBrandingCommandValidator : BrandingUpdateValidator<UpdateSchoolBrandingCommand>
{
    public UpdateSchoolBrandingCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
    }
}

public class UpdateSchoolBrandingCommandHandler : IRequestHandler<UpdateSchoolBrandingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateSchoolBrandingCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateSchoolBrandingCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);

        await BrandingStore.WriteAsync(_context, schoolId, request, cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BrandingChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: schoolId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
