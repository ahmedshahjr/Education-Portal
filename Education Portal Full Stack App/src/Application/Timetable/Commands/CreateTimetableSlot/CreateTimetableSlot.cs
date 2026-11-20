using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Timetable.Commands.CreateTimetableSlot;

/// <summary>
/// Adds a lesson of a course to the timetable. It is refused when the course's teacher, the room or the class is already
/// booked at that time (ADR-019). Needs the Timetable module and the Create right. Audited.
/// </summary>
[Authorize(Module = ModuleCode.Timetable, Right = AccessRight.Create)]
public record CreateTimetableSlotCommand : IRequest<Guid>
{
    public Guid CourseId { get; init; }

    public Weekday Day { get; init; }

    public TimeOnly Start { get; init; }

    public TimeOnly End { get; init; }

    public Guid? CampusId { get; init; }

    public string? Room { get; init; }
}

public class CreateTimetableSlotCommandValidator : AbstractValidator<CreateTimetableSlotCommand>
{
    public CreateTimetableSlotCommandValidator()
    {
        RuleFor(c => c.CourseId).NotEmpty();
        RuleFor(c => c.Day).IsInEnum();
        RuleFor(c => c.End).GreaterThan(c => c.Start).WithMessage(_ => ValidationMessages.TimetableEndsAfterStart);
        RuleFor(c => c.Room).MaximumLength(TimetableSlot.RoomMaxLength);
    }
}

public class CreateTimetableSlotCommandHandler : IRequestHandler<CreateTimetableSlotCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public CreateTimetableSlotCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateTimetableSlotCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var courseId = await _context.Courses.ResolveIdAsync(request.CourseId, cancellationToken);
        int? campusId = request.CampusId is { } campus ? await _context.Campuses.ResolveIdAsync(campus, cancellationToken) : null;

        var conflicts = await TimetableRules.FindConflictsAsync(
            _context, 0, courseId, request.Day, request.Start, request.End, campusId, request.Room, cancellationToken);
        if (conflicts.Count > 0)
        {
            throw TimetableRules.ToValidationException(conflicts);
        }

        var slot = TimetableSlot.Create(schoolId, courseId, campusId, request.Day, request.Start, request.End, request.Room);
        _context.TimetableSlots.Add(slot);
        await _context.SaveChangesAsync(cancellationToken);

        return slot.PublicId;
    }
}
