namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a school role may do in one module: see it, create, edit or delete what it holds, approve
/// what waits for a decision and export its lists (SEC-AUTHZ-02). <see cref="None"/>
/// means the request names no right. The numbers are fixed; never reuse one. Used in code only (no column stores it yet).
/// </summary>
public enum AccessRight
{
    None = 0,

    View = 1,

    Create = 2,

    Edit = 3,

    Delete = 4,

    Approve = 5,

    Export = 6,

    /// <summary>May read the change history of the module (ADR-023). Needs <see cref="View"/>.</summary>
    ViewHistory = 7,
}
