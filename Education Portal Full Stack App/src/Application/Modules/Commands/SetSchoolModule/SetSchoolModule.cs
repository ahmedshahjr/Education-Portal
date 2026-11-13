using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.SetSchoolModule;

/// <summary>
/// Switches a paid module on or off for one school. Core modules are always on, so they cannot be switched. The master
/// tenant itself cannot be changed. Until subscriptions exist (phase 3) this switch is how a school gets a paid module. A master-tenant use case, so the school id may come from the route
/// (SEC-AUTHZ-04, SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetSchoolModuleCommand : IRequest
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string Code { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}

public class SetSchoolModuleCommandValidator : AbstractValidator<SetSchoolModuleCommand>
{
    public SetSchoolModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.Code)
            .NotEmpty()
            .MaximumLength(PortalModule.CodeMaxLength)
            .MustAsync(async (code, cancellationToken) => !await context.Modules.AnyAsync(m => m.Code == code && m.IsCore, cancellationToken))
            .WithMessage(_ => ValidationMessages.CoreModuleAlwaysOn);

        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await context.Schools.AnyAsync(s => s.PublicId == schoolId && s.IsMaster, cancellationToken))
            .WithMessage(_ => ValidationMessages.MasterHasEveryModule);
    }
}

public class SetSchoolModuleCommandHandler : IRequestHandler<SetSchoolModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public SetSchoolModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(SetSchoolModuleCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);

        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Code == request.Code, cancellationToken);
        Guard.Against.NotFound(request.Code, module);

        var choice = await _context.SchoolModules
            .FirstOrDefaultAsync(c => c.SchoolId == schoolId && c.ModuleId == module.Id, cancellationToken);

        if (choice is null)
        {
            _context.SchoolModules.Add(SchoolModule.Create(schoolId, module, request.IsEnabled));
        }
        else
        {
            choice.SetEnabled(request.IsEnabled);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolModuleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: string.Create(CultureInfo.InvariantCulture, $"{schoolId}/{module.Code}"));
    }
}
