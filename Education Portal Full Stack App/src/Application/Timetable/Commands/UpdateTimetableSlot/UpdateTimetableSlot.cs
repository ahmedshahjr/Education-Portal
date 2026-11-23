using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Timetable.Commands.UpdateTimetableSlot;

/// <summary>
/// Moves a lesson to another day, time, room or campus. The lesson itself is never a clash with its own saved version.
/// Needs the Timetable module and the Edit right. Audited.
/// </summary>
[Authorize(Module = ModuleCode.Timetable, Right = AccessRight.Edit)]
public record UpdateTimetableSlotCommand
{
    public Guid Id { get; init; }

    public Weekday Day { get; init; }

    public TimeOnly Start { get; init; }

    public TimeOnly End { get; init; }

    public Guid CampusId { get; init; }

    public string? Room { get; init; }
}

public class UpdateTimetableSlotCommandValidator : AbstractValidator<UpdateTimetableSlotCommand>
{
    public UpdateTimetableSlotCommandValidator()
    {
        RuleFor(c => c.CampusId).NotEmpty();
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Day).IsInEnum();
        RuleFor(c => c.End).GreaterThan(c => c.Start).WithMessage(_ => ValidationMessages.TimetableEndsAfterStart);
        RuleFor(c => c.Room).MaximumLength(TimetableSlot.RoomMaxLength);
    }
}

public class UpdateTimetableSlotCommandHandler
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;

    public UpdateTimetableSlotCommandHandler(IApplicationDbContext context, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
    }

    public async Task Handle(UpdateTimetableSlotCommand request, CancellationToken cancellationToken)
    {
        var slotId = await _context.TimetableSlots.ResolveIdAsync(request.Id, cancellationToken);
        var slot = await _context.TimetableSlots.FirstAsync(s => s.Id == slotId, cancellationToken);
        var campusId = await _context.Campuses.ResolveIdAsync(request.CampusId, cancellationToken);
        await _campusAccess.EnsureCampusAsync(slot.CampusId, ModuleCode.Timetable, AccessRight.Edit, cancellationToken);
        await _campusAccess.EnsureCampusAsync(campusId, ModuleCode.Timetable, AccessRight.Edit, cancellationToken);

        var conflicts = await TimetableRules.FindConflictsAsync(
            _context, slotId, slot.CourseId, request.Day, request.Start, request.End, campusId, request.Room, cancellationToken);
        if (conflicts.Count > 0)
        {
            throw TimetableRules.ToValidationException(conflicts);
        }

        slot.Reschedule(campusId, request.Day, request.Start, request.End, request.Room);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
