using System.Security.Claims;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>
/// Identifies the caller for the test host. An HTTP request is identified by its session cookie, as in production. A test
/// that sends a MediatR request directly has no HTTP request, so it uses the user that <see cref="TestApp"/> chose.
/// </summary>
internal sealed class TestUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext is { } context
        ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        : TestApp.GetUserId();

    public List<string>? Roles => httpContextAccessor.HttpContext is { } context
        ? context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList()
        : TestApp.GetRoles();
}
