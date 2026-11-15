using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.CreateUserDetails;

/// <summary>
/// Adds a person (a principal, a teacher or other staff) to the current school. The school is the one the request's host
/// resolves to (SEC-AUTHZ-04). Needs the Staff module. Audited (SEC-LOG-03) with the person's id only, not the name.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Staff)]
public record CreateUserDetailsCommand : UserDetailsFields, IRequest<Guid>;

public class CreateUserDetailsCommandValidator : UserDetailsFieldsValidator<CreateUserDetailsCommand>
{
}

public class CreateUserDetailsCommandHandler : IRequestHandler<CreateUserDetailsCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateUserDetailsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var person = UserDetails.Create(schoolId, request.FirstName, request.MiddleName, request.LastName, request.UserType, request.Phone);
        _context.UserDetails.Add(person);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(UserDetails),
            targetId: person.Id.ToString(CultureInfo.InvariantCulture));

        return person.PublicId;
    }
}
