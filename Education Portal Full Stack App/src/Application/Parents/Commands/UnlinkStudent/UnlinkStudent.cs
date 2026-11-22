using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Parents.Commands.UnlinkStudent;

/// <summary>Removes the link between a parent and a student of the school. Needs the Staff module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record UnlinkStudentCommand
{
    public Guid ParentId { get; init; }

    public Guid StudentId { get; init; }
}

public class UnlinkStudentCommandValidator : AbstractValidator<UnlinkStudentCommand>
{
    public UnlinkStudentCommandValidator()
    {
        RuleFor(c => c.ParentId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
    }
}

public class UnlinkStudentCommandHandler
{
    private readonly IApplicationDbContext _context;

    public UnlinkStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UnlinkStudentCommand request, CancellationToken cancellationToken)
    {
        var parentId = await _context.UserDetails.ResolveIdAsync(request.ParentId, cancellationToken);
        var studentId = await _context.UserDetails.ResolveIdAsync(request.StudentId, cancellationToken);

        var link = await _context.ParentStudents.FirstOrDefaultAsync(
            l => l.ParentUserDetailsId == parentId && l.StudentUserDetailsId == studentId, cancellationToken);
        Guard.Against.NotFound(request.StudentId, link);

        _context.ParentStudents.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
