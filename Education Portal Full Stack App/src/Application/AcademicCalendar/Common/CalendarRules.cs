using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using FluentValidation.Results;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>Checks shared by the school calendar use cases.</summary>
public static class CalendarRules
{
    /// <summary>
    /// Resolves an optional campus public id to its internal id within the school. <c>null</c> means every campus. A campus
    /// of another school, or one that does not exist, is a 404 (SEC-AUTHZ-03).
    /// </summary>
    public static async Task<int?> ResolveOptionalCampusAsync(IApplicationDbContext context, int schoolId, Guid? campusId, CancellationToken cancellationToken) =>
        campusId is { } publicId
            ? await context.Campuses.Where(c => c.SchoolId == schoolId).ResolveIdAsync(publicId, cancellationToken)
            : null;

    /// <summary>Throws a validation error on <paramref name="property"/> with the given message.</summary>
    public static EducationPortal.Application.Common.Exceptions.ValidationException Invalid(string property, string message) =>
        new([new ValidationFailure(property, message)]);
}
