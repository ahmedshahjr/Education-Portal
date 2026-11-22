using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Parents.Commands.LinkStudent;

/// <summary>
/// Links a student of the school to one of its parents, so the parent can see them. Both must be people of the school, the
/// first a parent and the second a student. Needs the Staff module. Recorded by the change audit (ADR-016).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record LinkStudentCommand
{
    public Guid ParentId { get; init; }

    public Guid StudentId { get; init; }
}

public class LinkStudentCommandValidator : AbstractValidator<LinkStudentCommand>
{
    public LinkStudentCommandValidator()
    {
        RuleFor(c => c.ParentId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty().NotEqual(c => c.ParentId);
    }
}

public class LinkStudentCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public LinkStudentCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task Handle(LinkStudentCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var parentId = await _context.UserDetails.ResolveIdAsync(request.ParentId, cancellationToken);
        var studentId = await _context.UserDetails.ResolveIdAsync(request.StudentId, cancellationToken);

        var parent = await _context.UserDetails.AsNoTracking().FirstAsync(u => u.Id == parentId, cancellationToken);
        var student = await _context.UserDetails.AsNoTracking().FirstAsync(u => u.Id == studentId, cancellationToken);

        if (parent.UserType != UserType.Parent)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.ParentId), ValidationMessages.ParentIsNotParent)]);
        }

        if (student.UserType != UserType.Student)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.StudentId), ValidationMessages.StudentIsNotStudent)]);
        }

        var alreadyLinked = await _context.ParentStudents.AnyAsync(
            l => l.ParentUserDetailsId == parentId && l.StudentUserDetailsId == studentId, cancellationToken);
        if (alreadyLinked)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.StudentId), ValidationMessages.ParentAlreadyLinked)]);
        }

        _context.ParentStudents.Add(ParentStudent.Create(schoolId, parentId, studentId));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
