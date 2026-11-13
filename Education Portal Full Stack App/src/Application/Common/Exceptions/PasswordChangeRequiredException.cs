namespace EducationPortal.Application.Common.Exceptions;

/// <summary>
/// A request from an account that still has its one-time password. It is a 403 with the stable code
/// <see cref="ErrorCode"/>, so the client knows to show the change-password page instead of "you lack the right".
/// </summary>
public class PasswordChangeRequiredException : ForbiddenAccessException
{
    public const string ErrorCode = "PASSWORD_CHANGE_REQUIRED";
}
