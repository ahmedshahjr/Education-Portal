using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Common;

/// <summary>The text of a school's public page. Before the first save it is the empty draft (no tagline, not published).</summary>
public sealed record SchoolProfileDto(
    string Tagline,
    string About,
    int? EstablishedYear,
    SchoolGenderPolicy GenderPolicy,
    string? VisitingHours,
    string? VirtualTourUrl,
    bool IsPublished,
    DateTimeOffset? PublishedAt);

/// <summary>A facility on the public page. <see cref="Id"/> is the public id; the kind identifies it for the school.</summary>
public sealed record SchoolFacilityDto(Guid Id, FacilityKind Kind, string? Description);

/// <summary>A gallery photo without its bytes (fetched separately). <see cref="CampusId"/> is the campus's public id.</summary>
public sealed record SchoolPhotoDto(Guid Id, Guid? CampusId, PhotoKind Kind, string? Caption, int SortOrder, string ContentType, string Version);

/// <summary>The bytes of a photo with the content type checked from the file, and the version used for caching.</summary>
public sealed record SchoolPhotoContentDto(byte[] Content, string ContentType, string Version);

/// <summary>A class offering. <see cref="CampusId"/> is <c>null</c> when it applies to every campus.</summary>
public sealed record ClassOfferingDto(
    Guid Id,
    Guid? CampusId,
    ClassLevel Level,
    string AcademicYear,
    decimal MonthlyFee,
    decimal? AdmissionFee,
    string CurrencyCode,
    int SeatsTotal,
    int SeatsAvailable,
    AdmissionStatus Status,
    bool ShowFee);
