using System.Globalization;
using EducationPortal.Application.Branches.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Branches.Commands.CreateBranch;

/// <summary>
/// Adds a campus to the current school. The campus gets a four-digit code, generated here and unique within the school. The school
/// is the one the request's host resolves to (SEC-AUTHZ-04). Needs the Branches module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record CreateBranchCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
}

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Branch.NameMaxLength);
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateBranchCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var used = (await _context.Branches
                .Where(b => b.SchoolId == schoolId)
                .Select(b => b.Code)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        var code = CampusCodes.NextFree(used, Random.Shared)
            ?? throw new Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.Name),ValidationMessages.CampusCodesExhausted)]);

        var branch = Branch.Create(schoolId, request.Name, code);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BranchCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Branch),
            targetId: branch.Id.ToString(CultureInfo.InvariantCulture));

        return branch.PublicId;
    }
}
