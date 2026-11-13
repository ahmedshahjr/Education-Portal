namespace EducationPortal.Application.Common.Security;

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
    /// The code of the module the request belongs to (a <c>ModuleCodes</c> constant). When set, the request is refused with
    /// 403 unless the module is switched on for the school the request's host resolves to (SEC-AUTHZ-01). The master
    /// tenant is never limited by modules.
    /// </summary>
    public string Module { get; set; } = string.Empty;
}
