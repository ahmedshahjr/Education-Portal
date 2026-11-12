using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Localization;

namespace EducationPortal.Application.Countries.Queries.GetLanguages;

/// <summary>
/// The languages the portal has translations for, which the master tenant can offer in a country. Master tenant
/// administrators only (SEC-AUTHZ-07).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetLanguagesQuery : IRequest<IReadOnlyList<LanguageDto>>;

public class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, IReadOnlyList<LanguageDto>>
{
    public Task<IReadOnlyList<LanguageDto>> Handle(GetLanguagesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LanguageDto>>(Language.All.Select(LanguageDto.From).ToList());
}
