namespace EducationPortal.Domain.Enums;

/// <summary>What happened to an audited row (ADR-016). Stored as its number (ENG-ENUM-02).</summary>
public enum ChangeAuditAction
{
    Added = 1,

    Modified = 2,

    Removed = 3,
}
