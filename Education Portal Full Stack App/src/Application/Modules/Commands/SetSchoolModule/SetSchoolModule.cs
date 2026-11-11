using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.SetSchoolModule;

/// <summary>
/// Switches a module on or off for one school. The choice wins over the module's default from then on. Core modules and
/// the master tenant itself cannot be changed. A master-tenant use case, so the school id may come from the route
/// (SEC-AUTHZ-04, SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetSchoolModuleCommand : IRequest
{
    public int SchoolId { get; init; }

    public string Key { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}

public class SetSchoolModuleCommandValidator : AbstractValidator<SetSchoolModuleCommand>
{
    public SetSchoolModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).GreaterThan(0);
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PortalModule.KeyMaxLength);

        RuleFor(c => c.Key)
            .MustAsync(async (key, cancellationToken) => !await context.Modules.AnyAsync(m => m.Key == key && m.IsCore, cancellationToken))
            .WithMessage("A core module is always on and cannot be switched per school.");

        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await context.Schools.AnyAsync(s => s.Id == schoolId && s.IsMaster, cancellationToken))
            .WithMessage("The master tenant always has every module.");
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
        if (!await _context.Schools.AnyAsync(s => s.Id == request.SchoolId, cancellationToken))
        {
            throw new NotFoundException(request.SchoolId.ToString(CultureInfo.InvariantCulture), nameof(School));
        }

        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Key == request.Key, cancellationToken);
        Guard.Against.NotFound(request.Key, module);

        var choice = await _context.SchoolModules
            .FirstOrDefaultAsync(c => c.SchoolId == request.SchoolId && c.ModuleId == module.Id, cancellationToken);

        if (choice is null)
        {
            _context.SchoolModules.Add(SchoolModule.Create(request.SchoolId, module, request.IsEnabled));
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
            targetId: string.Create(CultureInfo.InvariantCulture, $"{request.SchoolId}/{module.Key}"));
    }
}
