using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.AddOnRequests;

/// <summary>Projects add-on requests to <see cref="AddOnRequestDto"/>; the tenant query filter limits what a school sees.</summary>
internal static class AddOnRequestReader
{
    public static IQueryable<AddOnRequestDto> Query(IApplicationDbContext context, int? schoolId = null, AddOnRequestStatus? status = null) =>
        from r in context.AddOnRequests.AsNoTracking()
        where schoolId == null || r.SchoolId == schoolId
        where status == null || r.Status == status
        join s in context.Schools on r.SchoolId equals s.Id
        join m in context.Modules on r.ModuleId equals m.Id
        orderby r.Created descending
        select new AddOnRequestDto(r.PublicId, s.PublicId, s.Name, m.Code, m.Name, r.Status, r.Created, r.DecidedAt);
}
