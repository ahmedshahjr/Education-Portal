namespace EducationPortal.Application.Common.Models;

public enum PasswordSignInStatus
{
    Succeeded,
    Failed,
    LockedOut,
    RequiresTwoFactor,
    NotAllowed,
}

/// <summary>
/// The outcome of a password sign-in. <see cref="UserId"/> is set when the email belongs to an account,
/// for the audit log only; callers must never tell the client whether an account exists (SEC-AUTHN-05).
/// </summary>
public sealed record PasswordSignInResult(PasswordSignInStatus Status, string? UserId);
