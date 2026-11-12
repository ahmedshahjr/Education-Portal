using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.SetSchoolModule;

/// <summary>
/// Switches a module on or off for one school. The choice wins over the module's default from then on. The master tenant
/// itself cannot be changed. A master-tenant use case, so the school id may come from the route
/// (SEC-AUTHZ-04, SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetSchoolModuleCommand : IRequest
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string Key { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}

public class SetSchoolModuleCommandValidator : AbstractValidator<SetSchoolModuleCommand>
{
    public SetSchoolModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PortalModule.KeyMaxLength);

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

        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Key == request.Key, cancellationToken);
        Guard.Against.NotFound(request.Key, module);

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
            targetId: string.Create(CultureInfo.InvariantCulture, $"{schoolId}/{module.Key}"));
    }
}
