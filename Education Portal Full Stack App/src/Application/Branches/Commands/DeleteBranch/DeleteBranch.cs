using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Branches.Commands.DeleteBranch;

/// <summary>
/// Deletes a branch of the current school. The row is kept (soft delete) and the branch disappears everywhere. The main branch
/// cannot be deleted, and a branch that still has module grants must have them turned off first. Needs the Branches module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record DeleteBranchCommand : IRequest
{
    /// <summary>The branch's public id, taken from the route (ENG-ID-02).</summary>
    public Guid BranchId { get; init; }
}

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;

    public DeleteBranchCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, ICurrentTenant tenant, IUser user)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _tenant = tenant;
        _user = user;
    }

    public async Task Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.BranchId, cancellationToken);
        Guard.Against.NotFound(request.BranchId, branch);

        if (branch.Kind == BranchKind.Main)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.BranchId), ValidationMessages.BranchMainNotDeletable)]);
        }

        var hasModules = await _context.TenantModules.AnyAsync(
            t => t.BranchId == branch.Id && t.Status != TenantModuleStatus.Expired, cancellationToken);
        if (hasModules)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.BranchId), ValidationMessages.BranchHasModules)]);
        }

        branch.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BranchDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(Branch),
            targetId: branch.Id.ToString(CultureInfo.InvariantCulture));
    }
}
