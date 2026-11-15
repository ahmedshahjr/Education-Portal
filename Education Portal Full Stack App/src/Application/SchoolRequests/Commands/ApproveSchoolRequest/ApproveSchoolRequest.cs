using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.SchoolRequests.Commands.ApproveSchoolRequest;

/// <summary>
/// The master tenant approves a pending school request: the school is registered (domain, country, first administrator, the
/// modules that are on by default and a 30-day free trial), exactly as the master adding a school does, and the request
/// becomes approved. The school's registration and the approval are saved together, so a request is never approved
/// without its school. Master tenant administrators only (SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record ApproveSchoolRequestCommand : IRequest<Guid>
{
    /// <summary>The request's public id, taken from the route (ENG-ID-02).</summary>
    public Guid RequestId { get; init; }

    /// <summary>The host name the school is reached on. The master sets it, because the request has none (AGENTS.md, section 2).</summary>
    public string Domain { get; init; } = string.Empty;

    /// <summary>The school's country, which prices its trial and its plans. Required.</summary>
    public Guid CountryId { get; init; }

    /// <summary>Where the school's data is kept, or <c>null</c> when not decided yet.</summary>
    public DataRegion? DataRegion { get; init; }

    /// <summary>The email the school's first administrator signs in with.</summary>
    public string AdministratorEmail { get; init; } = string.Empty;

    /// <summary>A one-time password the administrator replaces on first sign-in.</summary>
    public string AdministratorPassword { get; init; } = string.Empty;
}

public class ApproveSchoolRequestCommandValidator : AbstractValidator<ApproveSchoolRequestCommand>
{
    public ApproveSchoolRequestCommandValidator()
    {
        RuleFor(c => c.RequestId).NotEmpty();
        RuleFor(c => c.CountryId).NotEmpty();
    }
}

public class ApproveSchoolRequestCommandHandler : IRequestHandler<ApproveSchoolRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public ApproveSchoolRequestCommandHandler(
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

    public async Task<Guid> Handle(ApproveSchoolRequestCommand request, CancellationToken cancellationToken)
    {
        // One transaction: the school, its grants, its trial, its administrator and the request's approval are saved together,
        // or not at all. A failure leaves the request pending and creates nothing.
        var (school, requestId) = await _context.ExecuteInTransactionAsync(async token =>
        {
            var schoolRequest = await _context.SchoolRequests.FirstOrDefaultAsync(r => r.PublicId == request.RequestId, token);
            Guard.Against.NotFound(request.RequestId, schoolRequest);

            if (schoolRequest.Status != SchoolRequestStatus.Pending)
            {
                throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.RequestId), ValidationMessages.SchoolRequestNotPending)]);
            }

            // The same registration as the master adding a school, so the two paths cannot drift.
            var registered = await SchoolRegistration.RegisterAsync(
                _context,
                _identityService,
                _timeProvider,
                new SchoolRegistrationRequest(
                    schoolRequest.SchoolName,
                    request.Domain,
                    request.CountryId,
                    request.DataRegion,
                    request.AdministratorEmail,
                    request.AdministratorPassword),
                token);

            schoolRequest.Approve();
            await _context.SaveChangesAsync(token);

            return (registered, schoolRequest.Id);
        }, cancellationToken);

        // The audit records are written after the commit, so they never describe a school or approval that was rolled back.
        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserCreated,
            SecurityAuditOutcome.Success,
            targetType: "User",
            targetId: school.AdministratorId);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolRequestApproved,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRequest),
            targetId: requestId.ToString(CultureInfo.InvariantCulture));

        return school.PublicId;
    }
}
