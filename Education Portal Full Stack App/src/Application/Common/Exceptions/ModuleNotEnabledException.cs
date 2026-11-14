namespace EducationPortal.Application.Common.Exceptions;

/// <summary>
/// A request to a module the school does not have (SEC-AUTHZ-06). It is a 403 like any forbidden request, with the
/// stable code <see cref="ErrorCode"/> so clients can tell "buy this module" from "you lack the right".
/// </summary>
public class ModuleNotEnabledException : ForbiddenAccessException
{
    public const string ErrorCode = "MODULE_NOT_ENABLED";

    public ModuleNotEnabledException(string moduleCode)
    {
        ModuleCode = moduleCode;
    }

    /// <summary>The code of the module that is off, for example <c>FEES</c>.</summary>
    public string ModuleCode { get; }
}
