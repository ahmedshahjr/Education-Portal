using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Starts a school's subscription and returns its id: a plan and/or add-on modules, one billing cycle, and an optional
/// trial. The school gets the plan's modules, the add-ons, what they require and every core module, priced from its
/// country's price book; the subscription and its modules are saved together. A school has one subscription that is not
/// cancelled, and the master tenant itself needs none. Master tenant administrators only (SEC-AUTHZ-07), so the school id
/// may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateSubscriptionCommand : IRequest<Guid>
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    /// <summary>The plan's code, or empty when only add-on modules are bought.</summary>
    public string? PlanCode { get; init; }

    /// <summary>Codes of paid modules bought on top of the plan (or alone).</summary>
    public IReadOnlyList<string> Modules { get; init; } = [];

    public BillingCycle BillingCycle { get; init; }

    /// <summary>Days of free trial; 0 starts the subscription active at once.</summary>
    public int TrialDays { get; init; }
}

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.BillingCycle).IsInEnum();
        RuleFor(c => c.TrialDays).InclusiveBetween(0, Subscription.MaxTrialDays);
        RuleFor(c => c.PlanCode).MaximumLength(Plan.CodeMaxLength);

        RuleFor(c => c.Modules)
            .NotNull()
            .Must(modules => modules.Count <= 100)
            .Must(modules => modules.Distinct(StringComparer.Ordinal).Count() == modules.Count)
            .WithMessage(_ => ValidationMessages.ModuleListedTwice);

        RuleFor(c => c)
            .Must(c => !string.IsNullOrWhiteSpace(c.PlanCode) || c.Modules?.Count > 0)
            .WithMessage(_ => ValidationMessages.SubscriptionNeedsPlanOrModule)
            .OverridePropertyName(nameof(CreateSubscriptionCommand.PlanCode));

        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await context.Schools.AnyAsync(s => s.PublicId == schoolId && s.IsMaster, cancellationToken))
            .WithMessage(_ => ValidationMessages.MasterHasEveryModule)
            .MustAsync(async (schoolId, cancellationToken) => !await (
                    from s in context.Subscriptions
                    join school in context.Schools on s.SchoolId equals school.Id
                    where school.PublicId == schoolId && s.Status != SubscriptionStatus.Cancelled
                    select s.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.SubscriptionExists)
            .MustAsync(async (schoolId, cancellationToken) => !await context.Schools.AnyAsync(s => s.PublicId == schoolId && s.CountryId == null, cancellationToken))
            .WithMessage(_ => ValidationMessages.SchoolHasNoCountry);
    }
}

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        var countryCode = await _context.Countries.Where(c => c.Id == school.CountryId).Select(c => c.Code).FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.SchoolId, countryCode);

        var quote = await SubscriptionPlanner.QuoteAsync(
            _context, countryCode, request.PlanCode, request.Modules, request.BillingCycle, cancellationToken);

        var subscription = Subscription.Create(
            school.Id, quote.Plan, quote.PriceBook, request.BillingCycle, quote.Amount, _timeProvider.GetUtcNow(), request.TrialDays);
        foreach (var module in quote.Modules)
        {
            subscription.AddModule(module);
        }

        // One save: the subscription and its modules are written in one transaction.
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SubscriptionCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Subscription),
            targetId: subscription.Id.ToString(CultureInfo.InvariantCulture));

        return subscription.PublicId;
    }
}
