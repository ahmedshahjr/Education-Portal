using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Commands.SetAccountRoles;
using EducationPortal.Application.Users.Commands.SetPersonCampuses;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SavePersonAccess;

/// <summary>
/// Saves a person's roles and campuses together (R3.2): the account's school roles and the person's campus assignments
/// are set in one transaction, so either both change or neither does. Each part is validated and audited by its own
/// command (<see cref="SetAccountRolesCommand"/>, <see cref="SetPersonCampusesCommand"/>). Needs the Users &amp; Roles module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record SavePersonAccessCommand
{
    public Guid PersonId { get; init; }

    public IReadOnlyList<Guid> RoleIds { get; init; } = [];

    public IReadOnlyList<Guid> CampusIds { get; init; } = [];
}

public class SavePersonAccessCommandHandler(
    IApplicationDbContext context,
    IRequestRules rules,
    SetAccountRolesCommandHandler setAccountRoles,
    SetPersonCampusesCommandHandler setPersonCampuses)
{
    public async Task Handle(SavePersonAccessCommand request, CancellationToken cancellationToken)
    {
        // Inside one transaction: if the account has no login, or a role or campus is not the school's, nothing is saved.
        // Each part is checked and run as its own use case, so its rules apply exactly as they do on their own.
        await context.ExecuteInTransactionAsync(async token =>
        {
            var roles = new SetAccountRolesCommand { PersonId = request.PersonId, RoleIds = request.RoleIds };
            await rules.CheckAsync(roles, token);
            await setAccountRoles.Handle(roles, token);

            var campuses = new SetPersonCampusesCommand { PersonId = request.PersonId, CampusIds = request.CampusIds };
            await rules.CheckAsync(campuses, token);
            await setPersonCampuses.Handle(campuses, token);

            return true;
        }, cancellationToken);
    }
}
