using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.CreateModule;

/// <summary>
/// Adds a module to the catalogue and returns its code. A new module starts active and requires nothing. Master tenant
/// administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateModuleCommand : ModuleFields, IRequest<string>
{
    /// <summary>The stable code, for example <c>FEES</c>. It cannot be changed later.</summary>
    public string Code { get; init; } = string.Empty;
}

public class CreateModuleCommandValidator : ModuleFieldsValidator<CreateModuleCommand>
{
    /// <summary>"enabled" is the route of the current school's module list, so no module may be named like it.</summary>
    private const string ReservedCode = "ENABLED";

    public CreateModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Code)
            .Must(code => PortalModule.IsValidCode(code) && code != ReservedCode)
            .WithMessage(_ => ValidationMessages.ModuleCodeFormat)
            // A deleted module's code can be used again: its rows point to the old module by id, never by code.
            .MustAsync(async (code, cancellationToken) =>
                !await context.Modules.AnyAsync(m => m.Code == code, cancellationToken))
            .WithMessage(_ => ValidationMessages.ModuleCodeTaken);
    }
}

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public CreateModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<string> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = PortalModule.Create(request.Code, request.Name, request.Description, request.IsCore, request.SortOrder);

        _context.Modules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: module.Code);

        return module.Code;
    }
}
