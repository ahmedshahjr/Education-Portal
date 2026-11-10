# ADR-005: Languages and Regional Settings per Country

## Status

Accepted

## Date

2026-11-10

## Context

Schools are in different countries and need the portal in their own languages: a school in Pakistan offers English and Urdu, a school in the UAE offers English and Arabic. Arabic and Urdu are written right to left. Dates, numbers and money must follow the country's conventions, and times must be shown in the school's time zone. The master tenant (the platform owner) decides which languages each country offers. Each school is already resolved from the request's host (AGENTS.md section 2), so the server knows the school before anyone signs in.

The maintainer asked for .NET best practice on the server and the framework's own approach on the frontend.

## Decision

**Data.** The master tenant manages `Country` records (ISO 3166-1 code, name, the languages its schools offer, the default language, an IANA time zone and an ISO 4217 currency) and assigns each school to a country, or to none (English only, UTC). The languages a country can offer are the ones the portal has translations for, fixed in code (`Domain/Localization/Language.cs`: English, Arabic, Urdu). Adding a language means adding its translations, then listing it there.

**Server.** ASP.NET Core request localization (`UseRequestLocalization`) with one custom `RequestCultureProvider` (`TenantRequestCultureProvider`) that runs after tenant resolution. It picks the first language the school offers from: the visitor's saved choice (a `__Host-Culture` cookie in the standard `CookieRequestCultureProvider` format), then the browser's `Accept-Language` by quality, then the country's default. The culture is the language plus the country code (`ur-PK`, `ar-AE`), falling back to the language alone when the runtime has no such culture. Server text is localized with `IStringLocalizer` and `.resx` files under `src/Web/Resources`; FluentValidation's built-in messages follow `CurrentUICulture`.

**Frontend.** Angular's own i18n, `@angular/localize`, with **runtime translation loading**: one build; before the app is imported, `main.ts` asks `GET /api/Localization` for the language, loads `i18n/messages.<language>.json` with `loadTranslations`, registers Angular's locale data, and sets `<html lang dir>`. Templates use `i18n` attributes and code uses `$localize`, always with a custom id (`@@area.key`). `ng extract-i18n` writes the source file `src/locale/messages.json`; translations live next to it. Switching language saves the choice (`PUT /api/Localization/language`) and reloads the page. The layout direction follows the language; the theme's manual LTR/RTL switch is removed. Styles use logical CSS properties (`margin-inline-start`, `inset-inline-end`) so they mirror in right-to-left languages.

## Rationale

- **Per-tenant provider, not the built-in providers alone.** The built-in cookie, query string and header providers accept any supported culture. Here the allowed list depends on the school, so a Pakistani school must never answer in Arabic even if a cookie or browser asks for it. One provider that knows the tenant keeps that rule in one place, and the rest of the framework (`CurrentCulture`, `Content-Language`, `IStringLocalizer`, FluentValidation) works unchanged.
- **Runtime loading instead of build-time localization.** Angular's build-time option (`localize: true`) produces one full build per language, served from per-language paths (`/ar/`, `/ur/`). That multiplies build time and deployment size by the number of languages, needs server routing per path, and fits badly with schools whose language list is changed at runtime by the master tenant. Runtime loading keeps Angular's official tooling (`i18n` markers, `$localize`, `ng extract-i18n`, the same message format), is one build, and lets the server decide the language per school. The cost is that switching language reloads the page, which is rare and acceptable. Compiled-in translations would be slightly faster at startup; the translation files are small (a few KB).
- **Not a third-party library (ngx-translate, Transloco).** They allow switching without a reload, but they add a dependency outside the Angular team (SEC-DEP rules), a second message syntax next to Angular's own, and no extraction tool that Angular supports.
- **Language list in code, countries in the database.** A language is only usable once its translations ship with the app, so the list of supported languages changes with a release. Which country offers which of them is a business setting the master tenant changes without a release.

## Consequences

**Easier:**
- Adding a country, or changing which languages it offers, needs no release: the master tenant does it in the master portal, and schools follow on the next request.
- Server messages, validation errors and Angular screens are all in the same language, chosen in one place.
- Dates and numbers follow the country (`ur-PK`), and the time zone travels with the API's localization answer for use in screens.

**Harder:**
- Every new user-visible string needs an `i18n` marker or `$localize` with a custom id, and a translation in every `messages.<language>.json`; a unit test (`translations.spec.ts`) fails when a translation or placeholder is missing.
- Adding a new language needs its translations (frontend `messages.<code>.json`, backend `.resx`), Angular locale data in `app-locale.ts`, and an entry in `Language.All`.
- Translations are machine-quality first drafts until a native speaker reviews them.
