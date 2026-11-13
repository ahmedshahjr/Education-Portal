using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.AddModuleDependency;

/// <summary>
/// Makes one module require another, for example FEES requires STUDENTS. Refused when it would create a loop (the
/// required module already needs this one, directly or through others), when a module would require itself, and when a
/// core module would require a paid one. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record AddModuleDependencyCommand : IRequest
{
    /// <summary>The module that needs the other one, taken from the route.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>The code of the module it needs.</summary>
    public string RequiresCode { get; init; } = string.Empty;
}

public class AddModuleDependencyCommandValidator : AbstractValidator<AddModuleDependencyCommand>
{
    public AddModuleDependencyCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);

        RuleFor(c => c.RequiresCode)
            .NotEmpty()
            .MaximumLength(PortalModule.CodeMaxLength)
            .Must((command, requiresCode) => requiresCode != command.Code)
            .WithMessage(_ => ValidationMessages.ModuleCannotRequireItself)
            .MustAsync(async (requiresCode, cancellationToken) =>
                await context.Modules.AnyAsync(m => m.Code == requiresCode, cancellationToken))
            .WithMessage(_ => ValidationMessages.RequiredModuleNotFound)
            .CustomAsync(async (requiresCode, validation, cancellationToken) =>
            {
                var command = validation.InstanceToValidate;
                var modules = await context.Modules
                    .AsNoTracking()
                    .Where(m => m.Code == command.Code || m.Code == requiresCode)
                    .Select(m => new { m.Id, m.Code, m.IsCore })
                    .ToListAsync(cancellationToken);

                var module = modules.SingleOrDefault(m => m.Code == command.Code);
                var requires = modules.SingleOrDefault(m => m.Code == requiresCode);

                if (module is null || requires is null)
                {
                    return; // The handler answers 404 for an unknown module; the rule above covers the required one.
                }

                var dependencies = await ModuleCatalogReader.LoadDependenciesAsync(context, cancellationToken);

                if (dependencies.Contains((module.Id, requires.Id)))
                {
                    validation.AddFailure(ValidationMessages.ModuleDependencyExists);
                }
                else if (ModuleDependencyGraph.WouldCreateCycle(dependencies, module.Id, requires.Id))
                {
                    validation.AddFailure(ValidationMessages.ModuleDependencyCycle);
                }
                else if (module.IsCore && !requires.IsCore)
                {
                    validation.AddFailure(ValidationMessages.CoreModuleRequiresCoreOnly);
                }
            });
    }
}

public class AddModuleDependencyCommandHandler : IRequestHandler<AddModuleDependencyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public AddModuleDependencyCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(AddModuleDependencyCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Code == request.Code, cancellationToken);
        Guard.Against.NotFound(request.Code, module);

        var requires = await _context.Modules.FirstOrDefaultAsync(m => m.Code == request.RequiresCode, cancellationToken);
        Guard.Against.NotFound(request.RequiresCode, requires);

        _context.ModuleDependencies.Add(ModuleDependency.Create(module, requires));

        // Two masters adding opposite dependencies at once could each pass the loop check; the unique index stops
        // duplicates, and dependency resolution visits each module once, so a rare loop cannot hang it.
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleDependencyAdded,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: $"{module.Code}/{requires.Code}");
    }
}
