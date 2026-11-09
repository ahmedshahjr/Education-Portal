using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Users.Commands.SignIn;
using EducationPortal.Application.Users.Commands.SignOut;
using EducationPortal.Application.Users.Queries.GetCurrentUser;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(SignIn, "login")
            // compliance-exception: SEC-AUTHN-01 signing in is the one endpoint that runs before a session exists
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SignIn)
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(SignOut, "logout")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetCurrentUser, "me")
            .RequireAuthorization();
    }

    [EndpointSummary("Sign in")]
    [EndpointDescription("Signs the user in with email and password and sets the session cookie. Every failure returns 401 with the same message, so the response does not show whether an account exists or is locked.")]
    public static async Task<Results<Ok, ProblemHttpResult>> SignIn(ISender sender, SignInCommand command)
    {
        var signedIn = await sender.Send(command);

        return signedIn
            ? TypedResults.Ok()
            : TypedResults.Problem(
                title: "Unauthorized",
                detail: "Invalid email or password.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
    }

    [EndpointSummary("Sign out")]
    [EndpointDescription("Signs the current user out of every session, on every device, and clears the cookie of this browser.")]
    public static async Task<Ok> SignOut(ISender sender)
    {
        await sender.Send(new SignOutCommand());

        return TypedResults.Ok();
    }

    [EndpointSummary("Get the current user")]
    [EndpointDescription("Returns the signed-in user's own account. Returns 401 without a valid session.")]
    public static async Task<Ok<CurrentUserDto>> GetCurrentUser(ISender sender)
    {
        var user = await sender.Send(new GetCurrentUserQuery());

        return TypedResults.Ok(user);
    }
}
