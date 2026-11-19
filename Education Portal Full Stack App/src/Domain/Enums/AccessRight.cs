namespace EducationPortal.Domain.Enums;

/// <summary>
/// What a school role may do in one module: see it, create, edit or delete what it holds (SEC-AUTHZ-02). <see cref="None"/>
/// means the request names no right. The numbers are fixed; never reuse one. Used in code only (no column stores it yet).
/// </summary>
public enum AccessRight
{
    None = 0,

    View = 1,

    Create = 2,

    Edit = 3,

    Delete = 4,
}
