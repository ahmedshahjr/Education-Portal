namespace EducationPortal.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

    /// <summary>True while the signed-in account must replace its one-time password.</summary>
    bool MustChangePassword { get; }

}
