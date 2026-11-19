namespace EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;


/// <summary>
/// Specifies the class this attribute is applied to requires authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class. 
    /// </summary>
    public AuthorizeAttribute() { }

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
    /// </summary>
    public string Roles { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy name that determines access to the resource.
    /// </summary>
    public string Policy { get; set; } = string.Empty;

    /// <summary>
    /// When true, the request is a master-tenant (cross-school) use case and is allowed only on the master tenant's
    /// domain (SEC-AUTHZ-07). Other schools get 403.
    /// </summary>
    public bool MasterTenant { get; set; }

    /// <summary>
    /// The module the request belongs to (a <c>ModuleCode</c> member). When set, the request is refused with
    /// 403 unless the module is switched on for the school the request's host resolves to (SEC-AUTHZ-01). The master
    /// tenant is never limited by modules.
    /// </summary>
    public ModuleCode Module { get; set; } = ModuleCode.None;

    /// <summary>
    /// The right the request needs in <see cref="Module"/> (SEC-AUTHZ-02): a school role of the user must give it. Administrators
    /// and the master tenant are not checked. A request that names a right must also name its module.
    /// </summary>
    public AccessRight Right { get; set; } = AccessRight.None;

    /// <summary>
    /// When true, the request still runs while the user must replace a one-time password (reading their own account,
    /// changing the password, signing out). Every other request is refused with 403 and the code
    /// <c>PASSWORD_CHANGE_REQUIRED</c> until the password is changed.
    /// </summary>
    public bool AllowWhilePasswordChangeRequired { get; set; }
}
