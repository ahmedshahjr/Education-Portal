using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Constants;
using FluentValidation.Results;

namespace EducationPortal.Application.Schools.Commands.CreateSchool;

/// <summary>
/// Adds a school tenant on its own domain, with its country (languages, time zone, currency and price book, which must exist)
/// and data region, together with its first administrator. The administrator gets a one-time password from the master
/// and must replace it on first sign-in. School and account are saved in one transaction, so neither exists without the
/// other. The school starts with the default branding, the modules that are on by default, and a three-month (90-day) free trial of the Full
/// plan when it has a country. Master tenant administrators only
/// (SEC-AUTHZ-07); the master tenant itself is only ever created by seeding. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateSchoolCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;

    /// <summary>The host name the school is reached on, for example <c>school1.com</c>. Stored in lower case. Blank: the school has none and signs in on the master tenant's domain.</summary>
    public string Domain { get; init; } = string.Empty;

    /// <summary>The country's public id. Required: it sets the school's languages, time zone, currency and price book, and prices its trial.</summary>
    public Guid CountryId { get; init; }

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

        // Optional: a school without a domain signs in on the master tenant's domain.
        RuleFor(c => c.Domain)
            .Must(domain => School.IsValidDomain(Normalize(domain)))
            .WithMessage(_ => ValidationMessages.SchoolDomainFormat)
            .When(c => !string.IsNullOrWhiteSpace(c.Domain))
            .DependentRules(() =>
                RuleFor(c => c.Domain)
                    .MustAsync(async (domain, cancellationToken) =>
                    {
                        var normalized = Normalize(domain);
                        return !await context.Schools.AnyAsync(s => s.Domain == normalized, cancellationToken);
                    })
                    .WithMessage(_ => ValidationMessages.SchoolDomainTaken)
                    .When(c => !string.IsNullOrWhiteSpace(c.Domain)));

        RuleFor(c => c.CountryId)
            .NotEmpty()
            .MustAsync(async (countryId, cancellationToken) => await context.Countries.AnyAsync(c => c.PublicId == countryId, cancellationToken))
            .WithMessage(_ => ValidationMessages.CountryNotFound)
            .DependentRules(() =>
                RuleFor(c => c.CountryId)
                    .MustAsync(async (countryId, cancellationToken) => await (
                            from c in context.Countries
                            join b in context.PriceBooks on c.Code equals b.CountryCode
                            where c.PublicId == countryId
                            select b.Id).AnyAsync(cancellationToken))
                    .WithMessage(_ => ValidationMessages.CountryHasNoPriceBook));

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
        // One transaction: the school, its grants, its trial and its administrator are saved together or not at all.
        var registered = await _context.ExecuteInTransactionAsync(token => SchoolRegistration.RegisterAsync(
            _context,
            _identityService,
            _timeProvider,
            new SchoolRegistrationRequest(
                request.Name,
                request.Domain,
                request.CountryId,
                request.DataRegion,
                request.AdministratorEmail,
                request.AdministratorPassword),
            token), cancellationToken);

        // The audit records are written after the commit, so they never describe a school that was rolled back.
        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: registered.Id.ToString(CultureInfo.InvariantCulture));

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserCreated,
            SecurityAuditOutcome.Success,
            targetType: "User",
            targetId: registered.AdministratorId);

        return registered.PublicId;
    }
}
