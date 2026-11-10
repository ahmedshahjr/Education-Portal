using System.Net;
using System.Net.Http.Json;
using EducationPortal.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>
/// A browser-like client for HTTP tests. It keeps the cookies from Set-Cookie headers and sends them back, and it sends the
/// antiforgery token in the X-XSRF-TOKEN header on unsafe requests, as Angular does (SEC-WEB-03). Each session has its own
/// cookies, so one test can hold several signed-in sessions at once.
/// </summary>
internal sealed class HttpSession(HttpClient client) : IDisposable
{
    public const string SignInPath = "/api/Users/login";
    public const string SignOutPath = "/api/Users/logout";
    public const string CurrentUserPath = "/api/Users/me";

    private readonly Dictionary<string, string> _cookies = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Sends a header on every request, as a browser sends Accept-Language.</summary>
    public void SetHeader(string name, string value) => _headers[name] = value;

    public string? Cookie(string name) => _cookies.GetValueOrDefault(name);

    public void SetCookie(string name, string value) => _cookies[name] = value;

    public Task<HttpResponseMessage> GetAsync(string path) => SendAsync(HttpMethod.Get, path, body: null, withAntiforgeryToken: false);

    /// <summary>Sends a POST. The antiforgery token is fetched first, the way the Angular app does on load.</summary>
    public async Task<HttpResponseMessage> PostAsync(string path, object? body = null, bool withAntiforgeryToken = true)
    {
        if (withAntiforgeryToken && Cookie(AntiforgeryTokenCookie.CookieName) is null)
        {
            (await GetAsync("/")).Dispose();
        }

        return await SendAsync(HttpMethod.Post, path, body ?? new { }, withAntiforgeryToken);
    }

    /// <summary>
    /// Signs in. A token issued before sign-in belongs to the anonymous visitor, so after a successful sign-in the session
    /// asks for the current user. That response carries a token for the signed-in user, as the Angular client does.
    /// </summary>
    public async Task<HttpResponseMessage> SignInAsync(string email, string password)
    {
        var response = await PostAsync(SignInPath, new { email, password });

        if (response.StatusCode == HttpStatusCode.OK)
        {
            (await GetCurrentUserAsync()).Dispose();
        }

        return response;
    }

    /// <summary>Signs out. The session then asks for the current user, which refreshes the token for the anonymous visitor.</summary>
    public async Task<HttpResponseMessage> SignOutAsync()
    {
        var response = await PostAsync(SignOutPath);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            (await GetCurrentUserAsync()).Dispose();
        }

        return response;
    }

    public Task<HttpResponseMessage> GetCurrentUserAsync() => GetAsync(CurrentUserPath);

    /// <summary>Sends a JSON body with PUT, with the antiforgery token, as the Angular client does.</summary>
    public async Task<HttpResponseMessage> PutAsync(string path, object body)
    {
        if (Cookie(AntiforgeryTokenCookie.CookieName) is null)
        {
            (await GetAsync("/")).Dispose();
        }

        return await SendAsync(HttpMethod.Put, path, body, withAntiforgeryToken: true);
    }

    /// <summary>Sends a multipart form with PUT, with the antiforgery token, as the Angular client does for uploads.</summary>
    public async Task<HttpResponseMessage> PutFormAsync(string path, MultipartFormDataContent form)
    {
        if (Cookie(AntiforgeryTokenCookie.CookieName) is null)
        {
            (await GetAsync("/")).Dispose();
        }

        return await SendAsync(HttpMethod.Put, path, form, withAntiforgeryToken: true);
    }

    public void Dispose() => client.Dispose();

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, bool withAntiforgeryToken)
    {
        using var request = new HttpRequestMessage(method, path);

        if (_cookies.Count > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", _cookies.Select(cookie => $"{cookie.Key}={cookie.Value}")));
        }

        foreach (var (name, value) in _headers)
        {
            request.Headers.TryAddWithoutValidation(name, value);
        }

        if (withAntiforgeryToken && Cookie(AntiforgeryTokenCookie.CookieName) is { } token)
        {
            request.Headers.Add(AntiforgeryTokenCookie.HeaderName, token);
        }

        if (body is HttpContent content)
        {
            request.Content = content;
        }
        else if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await client.SendAsync(request);

        StoreCookies(response);

        return response;
    }

    private void StoreCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            return;
        }

        foreach (var setCookie in setCookies)
        {
            // The first segment is name=value. The attributes after it are not needed to send the cookie back.
            var pair = setCookie.Split(';', 2)[0];
            var separator = pair.IndexOf('=', StringComparison.Ordinal);

            if (separator <= 0)
            {
                continue;
            }

            var name = pair[..separator];
            var value = pair[(separator + 1)..];

            // A deleted cookie comes back with an empty value, so the session forgets it.
            if (value.Length == 0)
            {
                _cookies.Remove(name);
            }
            else
            {
                _cookies[name] = value;
            }
        }
    }
}

internal static class TestHttp
{
    public static HttpSession CreateDevelopmentSession(string host = TestApp.MasterDomain) =>
        CreateSession(FunctionalTestSetup.DevelopmentFactory, host);

    public static HttpSession CreateProductionSession() => CreateSession(FunctionalTestSetup.ProductionFactory);

    /// <summary>A session on one host. The host picks the tenant, as a school's domain does in production.</summary>
    public static HttpSession CreateSession(WebApplicationFactory<Program> factory, string host = TestApp.MasterDomain) =>
        new(factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Secure cookies are sent only over HTTPS, as in production.
            BaseAddress = new Uri($"https://{host}"),

            // The session keeps the cookies itself, so that redirects and cookie handling are visible to the test.
            HandleCookies = false,
            AllowAutoRedirect = false,
        }));
}
