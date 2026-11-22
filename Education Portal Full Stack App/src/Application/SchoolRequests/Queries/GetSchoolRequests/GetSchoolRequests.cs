using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolRequests.Queries.GetSchoolRequests;

/// <summary>A school request as the master tenant sees it in its queue.</summary>
public sealed record SchoolRequestDto(Guid Id, string SchoolName, string ContactName, string ContactEmail, SchoolRequestStatus Status);

/// <summary>
/// The school requests, pending ones first, the newest first within each status. Master tenant administrators only
/// (SEC-AUTHZ-07): the contact email is personal data of the requester (SEC-PII-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolRequestsQuery;

public class GetSchoolRequestsQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetSchoolRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolRequestDto>> Handle(GetSchoolRequestsQuery request, CancellationToken cancellationToken) =>
        await _context.SchoolRequests
            .AsNoTracking()
            .OrderBy(r => r.Status)
            .ThenByDescending(r => r.Created)
            .Select(r => new SchoolRequestDto(r.PublicId, r.SchoolName, r.ContactName, r.ContactEmail, r.Status))
            .ToListAsync(cancellationToken);
}
