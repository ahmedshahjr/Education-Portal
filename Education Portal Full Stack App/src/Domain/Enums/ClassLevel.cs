namespace EducationPortal.Domain.Enums;

/// <summary>
/// A class a school offers, from play group to A level. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or reuse a
/// number, and never rename a member.
/// </summary>
public enum ClassLevel
{
    Playgroup = 1,
    Nursery = 2,
    Prep = 3,
    Class1 = 4,
    Class2 = 5,
    Class3 = 6,
    Class4 = 7,
    Class5 = 8,
    Class6 = 9,
    Class7 = 10,
    Class8 = 11,
    Class9 = 12,
    Class10 = 13,
    FirstYear = 14,
    SecondYear = 15,
    OLevel = 16,
    ALevel = 17,
}
