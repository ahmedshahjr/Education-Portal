using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Communication;

/// <summary>
/// Who an event reaches (pure, no storage). Staff are every person who is not a student or a parent. An account with no
/// person record (a school administrator's account) reaches only events for everyone.
/// </summary>
public static class AudienceRules
{
    public static bool Reaches(EventAudience audience, UserType type) => audience switch
    {
        EventAudience.All => true,
        EventAudience.Staff => type is not (UserType.Student or UserType.Parent),
        EventAudience.Parents => type == UserType.Parent,
        EventAudience.Students => type == UserType.Student,
        _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "Not an audience."),
    };

    /// <summary>True when the event is shown to the person of this type; a null type is an account with no person record.</summary>
    public static bool Visible(EventAudience audience, UserType? type) =>
        type is { } person ? Reaches(audience, person) : audience == EventAudience.All;
}
