using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Commands.ChangePassword;
using EducationPortal.Application.Users.Commands.SignIn;
using EducationPortal.Application.Users.Commands.SignOut;
using EducationPortal.Application.Users.Queries.GetCurrentUser;
using EducationPortal.Application.Users.Queries.GetMyRights;
using EducationPortal.Web.Filters;
using EducationPortal.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EducationPortal.Web.Controllers;

[Route("api/Users")]
public class UsersController(
    IRequestRules rules,
    SignInCommandHandler signIn,
    SignOutCommandHandler signOut,
    GetCurrentUserQueryHandler getCurrentUser,
    GetMyRightsQueryHandler getMyRights,
    ChangePasswordCommandHandler changePassword) : ApiControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<SignInResponse>(StatusCodes.Status200OK)]
    // compliance-exception: SEC-AUTHN-01 signing in is the one endpoint that runs before a session exists
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.SignIn)]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Sign in")]
    [EndpointDescription("Signs the user in with email and password and sets the session cookie. The answer lists the modules the school may use, so the menu needs no second request. Every failure returns 401 with the same message, so the response does not show whether an account exists or is locked.")]
    public async Task<IActionResult> SignIn(SignInCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var outcome = await signIn.Handle(command, cancellationToken);
        return outcome.SignedIn
            ? Ok(new SignInResponse(outcome.Modules))
            : Problem(
                title: "Unauthorized",
                detail: "Invalid email or password.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Sign out")]
    [EndpointDescription("Signs the current user out of every session, on every device, and clears the cookie of this browser.")]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        var command = new SignOutCommand();
        await rules.CheckAsync(command, cancellationToken);
        await signOut.Handle(command, cancellationToken);
        return Ok();
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EndpointSummary("Get the current user")]
    [EndpointDescription("Returns the signed-in user's own account. Returns 401 without a valid session.")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCurrentUser.Handle(query, cancellationToken));
    }

    [HttpGet("me/rights")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EndpointSummary("Get my rights")]
    [EndpointDescription("Returns, for every module the school has, the rights the signed-in user's roles give and the campuses they hold on, for the menu and route guards. The server still checks every request. Returns 401 without a valid session.")]
    public async Task<ActionResult<MyRightsDto>> GetMyRights(CancellationToken cancellationToken)
    {
        var query = new GetMyRightsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getMyRights.Handle(query, cancellationToken));
    }

    [HttpPost("me/password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EnableRateLimiting(RateLimitPolicies.SignIn)] // it checks a password, so it gets the sign-in limit
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change my password")]
    [EndpointDescription("Replaces the signed-in user's password after checking the current one. Required before anything else for an account created with a one-time password. Ends the user's other sessions and renews this one. Answers 400 when the current password is wrong or the new one breaks the password policy.")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        await changePassword.Handle(command, cancellationToken);
        return NoContent();
    }
}
