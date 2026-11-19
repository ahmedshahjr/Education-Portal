using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Timetable.Queries.CheckTimetableConflicts;

/// <summary>
/// Shows the clashes a lesson would have, without saving it. Pass the lesson's id when it already exists, so it is not a
/// clash with itself. Needs the Timetable module and the View right.
/// </summary>
[Authorize(Module = ModuleCode.Timetable, Right = AccessRight.View)]
public record CheckTimetableConflictsQuery : IRequest<IReadOnlyList<TimetableConflictDto>>
{
    public Guid CourseId { get; init; }

    public Weekday Day { get; init; }

    public TimeOnly Start { get; init; }

    public TimeOnly End { get; init; }

    public Guid? CampusId { get; init; }

    public string? Room { get; init; }

    public Guid? SlotId { get; init; }
}

public class CheckTimetableConflictsQueryValidator : AbstractValidator<CheckTimetableConflictsQuery>
{
    public CheckTimetableConflictsQueryValidator()
    {
        RuleFor(c => c.CourseId).NotEmpty();
        RuleFor(c => c.Day).IsInEnum();
        RuleFor(c => c.End).GreaterThan(c => c.Start).WithMessage(_ => ValidationMessages.TimetableEndsAfterStart);
        RuleFor(c => c.Room).MaximumLength(TimetableSlot.RoomMaxLength);
    }
}

public class CheckTimetableConflictsQueryHandler : IRequestHandler<CheckTimetableConflictsQuery, IReadOnlyList<TimetableConflictDto>>
{
    private readonly IApplicationDbContext _context;

    public CheckTimetableConflictsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TimetableConflictDto>> Handle(CheckTimetableConflictsQuery request, CancellationToken cancellationToken)
    {
        var courseId = await _context.Courses.ResolveIdAsync(request.CourseId, cancellationToken);
        int? campusId = request.CampusId is { } campus ? await _context.Campuses.ResolveIdAsync(campus, cancellationToken) : null;
        var slotId = request.SlotId is { } slot ? await _context.TimetableSlots.ResolveIdAsync(slot, cancellationToken) : 0;

        return await TimetableRules.FindConflictsAsync(
            _context, slotId, courseId, request.Day, request.Start, request.End, campusId, request.Room, cancellationToken);
    }
}
