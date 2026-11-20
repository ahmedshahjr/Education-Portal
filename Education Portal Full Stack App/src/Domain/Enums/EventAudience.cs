namespace EducationPortal.Domain.Enums;

/// <summary>Who a school event is for. Stored as a number with a lookup table; sent over the API by name.</summary>
public enum EventAudience
{
    All = 1,

    Staff = 2,

    Parents = 3,

    Students = 4,
}
