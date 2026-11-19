using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;
using FluentValidation.Results;

namespace EducationPortal.Application.Schools.Commands.CreateSchool;

/// <summary>
/// Adds a school tenant on its own domain, optionally with its country (languages, time zone, currency and price book)
/// and data region, together with its first administrator. The administrator gets a one-time password from the master
/// and must replace it on first sign-in. School and account are saved in one transaction, so neither exists without the
/// other. The school starts with the default branding and no subscription. Master tenant administrators only
/// (SEC-AUTHZ-07); the master tenant itself is only ever created by seeding. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateSchoolCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;

    /// <summary>The host name the school is reached on, for example <c>school1.com</c>. Stored in lower case.</summary>
    public string Domain { get; init; } = string.Empty;

    /// <summary>The country's public id, or <c>null</c> for the platform defaults (English only, UTC).</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Where the school's data is kept, or <c>null</c> when not decided yet.</summary>
    public DataRegion? DataRegion { get; init; }

    /// <summary>The email the school's first administrator signs in with. Unique across the platform.</summary>
    public string AdministratorEmail { get; init; } = string.Empty;

    /// <summary>A one-time password the master passes on to the administrator, who must replace it on first sign-in.</summary>
    public string AdministratorPassword { get; init; } = string.Empty;
}

public class CreateSchoolCommandValidator : AbstractValidator<CreateSchoolCommand>
{
    public const int EmailMaxLength = 256;

    public CreateSchoolCommandValidator(IApplicationDbContext context, IIdentityService identityService)
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(School.NameMaxLength);

        RuleFor(c => c.Domain)
            .NotEmpty()
            .Must(domain => School.IsValidDomain(Normalize(domain)))
            .WithMessage(_ => ValidationMessages.SchoolDomainFormat)
            .DependentRules(() =>
                RuleFor(c => c.Domain)
                    .MustAsync(async (domain, cancellationToken) =>
                    {
                        var normalized = Normalize(domain);
                        return !await context.Schools.AnyAsync(s => s.Domain == normalized, cancellationToken);
                    })
                    .WithMessage(_ => ValidationMessages.SchoolDomainTaken));

        RuleFor(c => c.CountryId)
            .MustAsync(async (countryId, cancellationToken) => await context.Countries.AnyAsync(c => c.PublicId == countryId, cancellationToken))
            .When(c => c.CountryId is not null)
            .WithMessage(_ => ValidationMessages.CountryNotFound);

        RuleFor(c => c.DataRegion).IsInEnum();

        RuleFor(c => c.AdministratorEmail)
            .NotEmpty()
            .MaximumLength(EmailMaxLength)
            .EmailAddress()
            .DependentRules(() =>
                RuleFor(c => c.AdministratorEmail)
                    .MustAsync(async (email, _) => !await identityService.IsEmailTakenAsync(email.Trim()))
                    .WithMessage(_ => ValidationMessages.EmailTaken));

        // Identity checks the full password policy (common passwords, words from the account) when the account is made.
        RuleFor(c => c.AdministratorPassword)
            .NotEmpty()
            .MinimumLength(PasswordRules.MinimumLength)
            .MaximumLength(PasswordRules.MaximumLength);
    }

    /// <summary>The same normalisation the entity applies, so the uniqueness check compares like with like.</summary>
    private static string Normalize(string? domain) => (domain ?? string.Empty).Trim().TrimEnd('.').ToLowerInvariant();
}

public class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Guid>
{
    /// <summary>The free trial a new school starts with: three months on the full catalogue (ADR-011, the Full plan).</summary>
    public const int TrialDays = 90;

    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public CreateSchoolCommandHandler(IApplicationDbContext context, IIdentityService identityService, ISecurityAuditLog auditLog, TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
    {
        var school = School.Create(request.Name, request.Domain);

        if (request.CountryId is { } countryPublicId)
        {
            school.AssignCountry(await _context.Countries.ResolveIdAsync(countryPublicId, cancellationToken));
        }

        school.AssignDataRegion(request.DataRegion);

        var administratorId = await _context.ExecuteInTransactionAsync(async token =>
        {
            _context.Schools.Add(school);
            await _context.SaveChangesAsync(token);

            var (result, userId) = await _identityService.CreateSchoolAdministratorAsync(
                school.Id,
                request.AdministratorEmail.Trim(),
                request.AdministratorPassword);

            if (!result.Succeeded)
            {
                // Throwing rolls the school back too. The policy messages say what to change in the password.
                throw new Common.Exceptions.ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.AdministratorPassword), e)));
            }

            // The trial is part of the same save: a school never exists without its first administrator or its trial.
            await StartTrialAsync(school, token);

            return userId;
        }, cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserCreated,
            SecurityAuditOutcome.Success,
            targetType: "User",
            targetId: administratorId);

        return school.PublicId;
    }

    /// <summary>
    /// Starts the school's free trial: the Full plan for <see cref="TrialDays"/> days, billed monthly afterwards, with
    /// every module it gives. It needs an active price book and the Full plan for the school's country; without them
    /// the school is still created and the master tenant starts its subscription once prices exist. Audited.
    /// </summary>
    private async Task StartTrialAsync(School school, CancellationToken cancellationToken)
    {
        if (school.CountryId is not { } countryId)
        {
            return;
        }

        var countryCode = await _context.Countries.Where(c => c.Id == countryId).Select(c => c.Code).SingleAsync(cancellationToken);
        var hasPrices = await _context.PriceBooks.AnyAsync(b => b.CountryCode == countryCode && b.IsActive, cancellationToken)
            && await _context.Plans.AnyAsync(p => p.Code == PricingCatalog.Full && p.IsActive, cancellationToken);
        if (!hasPrices)
        {
            return;
        }

        var quote = await SubscriptionPlanner.QuoteAsync(_context, countryCode, PricingCatalog.Full, [], BillingCycle.Monthly, cancellationToken);
        var subscription = Subscription.Create(school.Id, quote.Plan, quote.PriceBook, BillingCycle.Monthly, quote.Amount, _timeProvider.GetUtcNow(), TrialDays);

        var mainBranch = await SchoolBranches.MainAsync(_context, school.Id, cancellationToken);
        foreach (var module in quote.Modules)
        {
            subscription.AddModule(module, mainBranch);
        }

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SubscriptionCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Subscription),
            targetId: subscription.Id.ToString(CultureInfo.InvariantCulture));
    }
}
