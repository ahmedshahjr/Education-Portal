using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Branches.Commands.UpdateBranch;

/// <summary>
/// Renames a campus of the current school. Its code is generated once and does not change. Needs the Branches module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record UpdateBranchCommand : IRequest
{
    /// <summary>The campus's public id, taken from the route (ENG-ID-02).</summary>
    public Guid BranchId { get; init; }

    public string Name { get; init; } = string.Empty;
}

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(c => c.BranchId).NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Branch.NameMaxLength);
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateBranchCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.BranchId, cancellationToken);
        Guard.Against.NotFound(request.BranchId, branch);

        branch.Rename(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BranchUpdated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Branch),
            targetId: branch.Id.ToString(CultureInfo.InvariantCulture));
    }
}
