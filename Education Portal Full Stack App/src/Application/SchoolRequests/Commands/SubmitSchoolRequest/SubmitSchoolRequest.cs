using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;

/// <summary>
/// The public "request a school" form, self-service (decided 2026-11-19): anyone can register a school. It creates the school
/// with its country's data region, its default modules, a 3-month free trial of the Full plan and its first administrator
/// (the contact email, with the password the visitor chose), all in one transaction. No master approval is needed. The
/// request is recorded as submitted. Anonymous, so it is rate limited per address (SEC-WEB-07). Audited (SEC-LOG-03).
/// </summary>
public record SubmitSchoolRequestCommand : IRequest<Guid>
{
    public string SchoolName { get; init; } = string.Empty;

    /// <summary>The name of the person who registers the school.</summary>
    public string ContactName { get; init; } = string.Empty;

    /// <summary>The email of the school's first administrator, who signs in with it.</summary>
    public string ContactEmail { get; init; } = string.Empty;

    /// <summary>The school's country, which sets its data region, prices its trial and its plans. Required.</summary>
    public Guid CountryId { get; init; }

    /// <summary>The password the administrator signs in with. The password policy is checked on registration.</summary>
    public string AdministratorPassword { get; init; } = string.Empty;
}

public class SubmitSchoolRequestCommandValidator : AbstractValidator<SubmitSchoolRequestCommand>
{
    public SubmitSchoolRequestCommandValidator()
    {
        RuleFor(r => r.SchoolName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.SchoolNameMaxLength);

        RuleFor(r => r.ContactName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactNameMaxLength);

        RuleFor(r => r.ContactEmail)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactEmailMaxLength)
            .EmailAddress();

        RuleFor(r => r.CountryId).NotEmpty();

        RuleFor(r => r.AdministratorPassword)
            .NotEmpty()
            .MinimumLength(SchoolRegistration.PasswordMinLength)
            .MaximumLength(SchoolRegistration.PasswordMaxLength);
    }
}

public class SubmitSchoolRequestCommandHandler : IRequestHandler<SubmitSchoolRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public SubmitSchoolRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        ISecurityAuditLog auditLog,
        TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(SubmitSchoolRequestCommand request, CancellationToken cancellationToken)
    {
        // One transaction: the school, its grants, its trial, its administrator and the request are saved together or not at all.
        var registered = await _context.ExecuteInTransactionAsync(async token =>
        {
            var school = await SchoolRegistration.RegisterAsync(
                _context,
                _identityService,
                _timeProvider,
                new SchoolRegistrationRequest(
                    request.SchoolName,
                    Domain: null,
                    request.CountryId,
                    request.ContactEmail,
                    request.AdministratorPassword,
                    MustChangePassword: false),
                token);

            var schoolRequest = SchoolRequest.Submit(request.SchoolName, request.ContactName, request.ContactEmail);
            schoolRequest.Approve();
            _context.SchoolRequests.Add(schoolRequest);
            await _context.SaveChangesAsync(token);

            return school;
        }, cancellationToken);

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
