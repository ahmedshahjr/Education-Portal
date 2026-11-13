using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Commands.SetDataRegion;

/// <summary>
/// Sets where a school's data is kept, or clears it with an empty value. Master tenant administrators only
/// (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetDataRegionCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public DataRegion? DataRegion { get; init; }
}

public class SetDataRegionCommandValidator : AbstractValidator<SetDataRegionCommand>
{
    public SetDataRegionCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.DataRegion).IsInEnum();
    }
}

public class SetDataRegionCommandHandler : IRequestHandler<SetDataRegionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public SetDataRegionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(SetDataRegionCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        school.AssignDataRegion(request.DataRegion);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.DataRegionChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
