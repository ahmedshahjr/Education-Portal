using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Modules.Commands.SetSchoolModule;

/// <summary>
/// The master tenant turns any module on or off for one school, core and default modules included, from the school's
/// modules screen. On: the school gets the whole school's grant of the module, free of charge, with no end. Off: every
/// active grant of the module ends now. The module's data is kept. A module the master switched off for every school
/// cannot be turned on. The master tenant itself has every module and is not changed here. Master tenant administrators
/// only (SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetSchoolModuleCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    /// <summary>The module's code, taken from the route.</summary>
    public string ModuleCode { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}

public class SetSchoolModuleCommandValidator : AbstractValidator<SetSchoolModuleCommand>
{
    public SetSchoolModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.ModuleCode).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);

        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await context.Schools.AnyAsync(s => s.PublicId == schoolId && s.IsMaster, cancellationToken))
            .WithMessage(_ => ValidationMessages.MasterHasEveryModule);

        RuleFor(c => c.ModuleCode)
            .MustAsync(async (code, cancellationToken) => await context.Modules.AnyAsync(m => m.Code == code, cancellationToken))
            .WithMessage(_ => ValidationMessages.UnknownModules);

        RuleFor(c => c)
            .MustAsync(async (command, cancellationToken) => !command.Enabled || await context.Modules.AnyAsync(m => m.Code == command.ModuleCode && m.IsActive, cancellationToken))
            .WithMessage(_ => ValidationMessages.ModuleSwitchedOff)
            .OverridePropertyName(nameof(SetSchoolModuleCommand.Enabled));
    }
}

public class SetSchoolModuleCommandHandler : IRequestHandler<SetSchoolModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IModuleEntitlementStore _entitlements;
    private readonly TimeProvider _timeProvider;

    public SetSchoolModuleCommandHandler(
        IApplicationDbContext context,
        ISecurityAuditLog auditLog,
        IModuleEntitlementStore entitlements,
        TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _entitlements = entitlements;
        _timeProvider = timeProvider;
    }

    public async Task Handle(SetSchoolModuleCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        var module = await _context.Modules.AsNoTracking().SingleAsync(m => m.Code == request.ModuleCode, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        // The grant change is saved in one transaction, or not at all.
        await _context.ExecuteInTransactionAsync(async token =>
        {
            if (request.Enabled)
            {
                await TurnOnAsync(school.Id, module.Id, now, token);
            }
            else
            {
                await TurnOffAsync(school.Id, module.Id, now, token);
            }

            await _context.SaveChangesAsync(token);
            return true;
        }, cancellationToken);

        // The school's entitlements just changed, so its cached copy is stale.
        _entitlements.Invalidate(school.Id);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolModuleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }

    private async Task TurnOnAsync(int schoolId, int moduleId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var holdsWholeSchool = await _context.TenantModules.IgnoreQueryFilters().AnyAsync(
            t => t.SchoolId == schoolId && t.ModuleId == moduleId && t.Status == TenantModuleStatus.Active
                 && t.IsSchoolWide && (t.ValidTo == null || t.ValidTo > now),
            cancellationToken);
        if (holdsWholeSchool)
        {
            return;
        }

        var mainCampus = await SchoolCampuses.MainAsync(_context, schoolId, cancellationToken);
        var module = await _context.Modules.SingleAsync(m => m.Id == moduleId, cancellationToken);

        // A default grant of the main campus that was ended is reopened, not duplicated: one such grant per module and school.
        var endedDefault = await _context.TenantModules
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(t => t.SchoolId == schoolId && t.ModuleId == moduleId && t.CampusId == mainCampus.Id && t.SubscriptionId == null, cancellationToken);

        if (endedDefault is not null)
        {
            endedDefault.Reopen(now);
        }
        else
        {
            _context.TenantModules.Add(TenantModule.CreateDefault(schoolId, module, mainCampus, now));
        }
    }

    private async Task TurnOffAsync(int schoolId, int moduleId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var grants = await _context.TenantModules
            .IgnoreQueryFilters()
            .Where(t => t.SchoolId == schoolId && t.ModuleId == moduleId && t.Status == TenantModuleStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var grant in grants)
        {
            grant.Expire(now);
        }
    }
}
