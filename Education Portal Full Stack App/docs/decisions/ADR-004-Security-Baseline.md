# ADR-004: Security Baseline for Sign-In and Sessions

## Status

Accepted

## Date

2026-11-09

## Context

Step 2 of the MVP plan (AGENTS.md, section 8) adds schools, branches and staff accounts. Before any screen can sign a user in, the secure-compliance rules in `docs/ai/secure-compliance/SKILL.md` require the following:

- ASP.NET Core Identity with its password hasher (SEC-AUTHN-02), a password policy (SEC-AUTHN-03) and account lockout (SEC-AUTHN-05).
- Session cookies that are `Secure`, `HttpOnly`, `SameSite` and host-only, with idle and absolute lifetimes (SEC-AUTHN-06, SEC-AUTHN-07).
- Antiforgery protection on every state-changing endpoint (SEC-WEB-03) and security headers on every response (SEC-WEB-04).
- Rate limiting on sign-in (SEC-WEB-07) and audit events for sign-in and sign-out (SEC-LOG-03).

The template ships a public register endpoint, the Identity API endpoints and a Google Fonts link. The content-security policy this baseline needs cannot allow the font link.

## Decision

1. **Sign-in** is a custom `POST /api/Users/login` that uses `SignInManager` and counts failed attempts. Every failure returns 401 with the same message, so the response does not show whether an account exists or is locked. For an unknown email the password check still runs against a dummy hash, so the response time does not show it either.
2. **Passwords** are 15 to 128 characters long. A password is rejected when it appears in the common-password list (SecLists xato-net 10-million, kept to entries of 15 characters or more, embedded gzip-compressed with its MIT notice) or contains the product name or the local part of the email address.
3. **Lockout**: three failed sign-ins lock the account for 30 minutes. The failure counter does not expire with time, so the account can lock sooner than SEC-AUTHN-05 requires, never later.
4. **Session cookie** is `__Host-EducationPortal`: `HttpOnly`, `Secure`, `SameSite=Strict`, host-only and on path `/`. It expires after 15 minutes of inactivity (sliding). The absolute lifetime is 12 hours from the original sign-in, kept in the ticket, so renewals do not extend it.
5. **Sign-out** rotates the user's security stamp. That ends every session of the user on every device, and this browser's cookie is cleared. The security stamp is checked on every request (`ValidationInterval` is zero), so a change takes effect on the next request.
6. **Antiforgery**: the `__Host-XSRF-TOKEN` cookie is readable by script. The Angular client copies it into the `X-XSRF-TOKEN` header of every unsafe request. Every endpoint group gets an endpoint filter that rejects an unsafe request whose token is missing or wrong, before the handler runs. Because a token is bound to the identity it was issued for, the client asks for `GET /api/Users/me` after sign-in and after sign-out to receive a token for the new identity.
7. **Security headers** are written in `OnStarting`, so they also appear on error responses. `Program.cs` calls authentication and authorization explicitly after the header middleware, because `WebApplication` would otherwise place them before it and a 401 would have no headers. The policy is `default-src 'self'`, `script-src 'self'` with no inline scripts, and `frame-ancestors 'none'`. The theme script moved to `assets/theme-init.js`, and the Google Fonts links were removed.
8. **Rate limit**: five sign-in attempts per minute per client address (fixed window), set by `RateLimiting:SignInPermitLimit`. The address is the direct peer. Forwarded headers are not trusted (SEC-WEB-08).
9. **Audit**: sign-in success and failure, lockout and sign-out are written to the `SecurityAuditEvents` table. Each write uses its own scope, so the record is kept when the request fails. The application has no update or delete use case for this table, and test resets leave it alone.
10. **Development only**: the seeded `administrator@localhost` account, the OpenAPI document and the Scalar UI exist only in Development (SEC-CFG-01, SEC-WEB-05). `InitialiseDatabaseAsync` throws outside Development.
11. **No public sign-up**: the register endpoint and page are removed. Accounts are created by staff (SEC-AUTHN-08), and that feature comes in step 2b.

## Rationale

- **Security stamp rather than a server-side session store.** The stamp is part of ASP.NET Core Identity, so no new table or cache is needed for the MVP. It ends all sessions of a user at once, which is stronger than sign-out of one device. Per-device revocation would need a session store, and that is deferred.
- **Cookies rather than bearer tokens.** A token kept in script-readable storage can be stolen by any script on the page. An `HttpOnly` cookie cannot be read by script, and the server can revoke it through the stamp. A JWT cannot be revoked without a store, so it was rejected.
- **Custom sign-in endpoint rather than the template's Identity API endpoints.** The template endpoints cover registration, password reset, two-factor setup and bearer tokens. Keeping only what the MVP needs follows SEC-WEB-09. Switching the template endpoints off instead was rejected, because a missed one would stay reachable.
- **An endpoint filter as well as the antiforgery middleware.** For JSON endpoints the middleware only records the result, and the endpoint still runs. A test showed that a signed-in sign-out without the header returned 200. The filter reads the recorded result first, because reading the form after an invalid result throws `InvalidOperationException`, and then validates the token itself.
- **Identity's lockout counter.** A windowed counter would need a custom store. The built-in counter is simpler and is stricter than the rule requires.

## Consequences

**Easier:**
- Sign-in, sign-out and session rules are in one place (`IdentityService`, `Infrastructure/DependencyInjection.cs`). Functional tests over HTTP cover the cookie flags, the lockout, the rate limit, the antiforgery check, sign-out of every session and a changed cookie.
- The Angular client makes no request to a third-party host.

**Harder, and not done in this step:**
- **MFA (SEC-AUTHN-04) is not implemented.** It must be in place before any master-tenant user, school owner or privileged role exists, which means step 2b, before any such account is created. This step creates no such account outside Development. Deferring it needs maintainer approval under section 15 of the rules.
- **The lockout counter has no 15-minute window** (SEC-AUTHN-05 asks for one). The lockout is stricter, so this does not weaken it.
- **Rate limiting is per client address only.** The per-account limit is the lockout (SEC-WEB-07 asks for both partitions).
- **Tenant resolution from the request host** (SEC-AUTHN-09, SEC-WEB-08), the school and branch claims, and the `SchoolId` and `BranchId` values in audit events are step 2b work. The audit columns exist and stay empty until then.
- **EF Core migrations are not in the repository yet.** Schema changes in shared environments must wait for them (SEC-CFG-03).
- **Password reset, invite links and disabling users** (SEC-AUTHN-08, SEC-AUTHN-10) are not implemented.
- **Fonts** fall back to system fonts until they are self-hosted, because external font hosts are blocked.
- **The app must see HTTPS on every request.** The antiforgery cookie is `Secure`, so over plain HTTP every request answers 500 (checked on 2026-11-09). Development serves HTTPS with the Kestrel development certificate. Hosting must give the app HTTPS, directly or through a proxy. Forwarded headers are trusted only from a configured proxy (SEC-WEB-08). Both are hosting decisions (AGENTS.md section 3, TODO).
- **The acceptance tests (Playwright and Reqnroll) are not verified.** In the sandbox used for this change, all three scenarios fail, and the base commit also fails at Aspire setup. A maintainer run of the acceptance suite is needed before merge.
