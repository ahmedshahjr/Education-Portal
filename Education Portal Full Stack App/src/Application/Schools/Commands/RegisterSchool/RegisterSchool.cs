using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Schools.Commands.RegisterSchool;

/// <summary>
/// Self-service school registration: the public form on the master tenant's domain. It creates the school, its first
/// administrator (with the password the administrator chose) and a free trial of the Full plan
/// (<see cref="SchoolRegistration.TrialDays"/> days) in one transaction, with no approval. The school has no domain of its own,
/// so it signs in on the master tenant's domain until the master gives it one (ADR-018). It is the one command that creates
/// a school without authorization, so the endpoint is rate limited and only the master tenant's domain accepts it. Audited (SEC-LOG-03).
/// </summary>
public record RegisterSchoolCommand
{
    public string SchoolName { get; init; } = string.Empty;

    /// <summary>The country's public id. It sets the school's languages, time zone, currency and the price book that prices the trial.</summary>
    public Guid CountryId { get; init; }

    /// <summary>The email the administrator signs in with. Unique across the platform.</summary>
    public string AdministratorEmail { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}

public class RegisterSchoolCommandValidator : AbstractValidator<RegisterSchoolCommand>
{
    public const int EmailMaxLength = 256;

    public RegisterSchoolCommandValidator(IApplicationDbContext context, IIdentityService identityService)
    {
        RuleFor(c => c.SchoolName)
            .NotEmpty()
            .MaximumLength(School.NameMaxLength);

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

        RuleFor(c => c.AdministratorEmail)
            .NotEmpty()
            .MaximumLength(EmailMaxLength)
            .EmailAddress()
            .DependentRules(() =>
                RuleFor(c => c.AdministratorEmail)
                    .MustAsync(async (email, _) => !await identityService.IsEmailTakenAsync(email.Trim()))
                    .WithMessage(_ => ValidationMessages.EmailTaken));

        // Identity checks the full password policy (common passwords, words from the account) when the account is made.
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(PasswordRules.MinimumLength)
            .MaximumLength(PasswordRules.MaximumLength);

        RuleFor(c => c.ConfirmPassword)
            .Equal(c => c.Password)
            .WithMessage(_ => ValidationMessages.PasswordsDoNotMatch);
    }
}

public class RegisterSchoolCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly TimeProvider _timeProvider;

    public RegisterSchoolCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        ISecurityAuditLog auditLog,
        ICurrentTenant tenant,
        TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _auditLog = auditLog;
        _tenant = tenant;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(RegisterSchoolCommand request, CancellationToken cancellationToken)
    {
        // Only the master tenant's domain takes registrations: a school's own domain is not a place to open another school.
        if (!_tenant.IsMaster)
        {
            throw new NotFoundException("host", nameof(School));
        }

        // One transaction: the school, its grants, its trial and its administrator are saved together or not at all.
        var registered = await _context.ExecuteInTransactionAsync(token => SchoolRegistration.RegisterAsync(
            _context,
            _identityService,
            _timeProvider,
            new SchoolRegistrationRequest(
                request.SchoolName,
                Domain: null,
                request.CountryId,
                DataRegion: null,
                request.AdministratorEmail,
                request.Password,
                AdministratorMustChangePassword: false),
            token), cancellationToken);

        // The audit records are written after the commit, so they never describe a school that was rolled back.
        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCreated,
            SecurityAuditOutcome.Success,
            actorUserId: registered.AdministratorId,
            targetType: nameof(School),
            targetId: registered.Id.ToString(CultureInfo.InvariantCulture));

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserCreated,
            SecurityAuditOutcome.Success,
            actorUserId: registered.AdministratorId,
            targetType: "User",
            targetId: registered.AdministratorId);

        return registered.PublicId;
    }
}
