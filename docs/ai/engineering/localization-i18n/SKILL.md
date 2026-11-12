---
name: localization-i18n
description: Internationalization and localization rules for the .NET backend and the Angular frontend, following Microsoft's globalization and localization guidance and Angular's i18n guide (externalized text with ar/ur translations, resx and $localize with custom ids, no concatenation, ICU plurals, culture-aware formatting, UTC storage with the user's time zone on display, right-to-left layout, per-school and per-user language, translation tests). Use when adding or changing any user-visible text, message, date, time, number, currency, layout direction, language or culture setting, or a translation file.
---

# Localization and internationalization (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

The design is decided in [ADR-005](../../../Education%20Portal%20Full%20Stack%20App/docs/decisions/ADR-005-Localization-Per-Country.md): the master tenant chooses each country's languages, the server answers each request in one of the school's languages, Angular loads its translations at run time, and Arabic and Urdu are laid out right to left. These rules say how to write code that keeps that working. Words: **internationalization (i18n)** makes the code ready for any language and region; **localization (l10n)** adds one language's text and conventions.

## 1. World-ready by default

| ID | Rule |
|---|---|
| ENG-I18N-01 | **Every change is world-ready from the start**, even if it ships in English only first. Retrofitting later costs more than doing it now. A screen, message or export that only works in English, left to right, in one time zone is not done (ENG-PROC-02). |
| ENG-I18N-02 | **Unicode everywhere.** Never limit user data (names, addresses, free text) to a script, a font or ASCII. Text is UTF-8 on the wire and in files. PostgreSQL `text`/`varchar` hold any script. Validators check lengths, not alphabets, unless the field is a code (ISO country, currency, language). |
| ENG-I18N-03 | **Culture-neutral data, culture-specific display.** Store and send instants as UTC (`DateTimeOffset`, ISO 8601 on the API), numbers and money as numbers (amount plus ISO 4217 code), codes as codes. Format only at the edge where a person reads it. Never store a formatted date, number or translated label. |
| ENG-I18N-04 | **Respect market conventions:** name order, postal address and telephone formats, units, first day of week and calendars differ by country. Do not hard-code one country's format (for example a fixed `dd/MM/yyyy` or a 10-digit phone rule). Take the format from the culture or the school's country settings. |

## 2. Text

| ID | Rule |
|---|---|
| ENG-I18N-10 | **Externalize every user-visible string.** That covers screens, labels, `aria-label`, `title`, placeholders, tooltips, snackbars, dialog text, validation and error messages, emails and exports. Nothing a user reads is a literal in code. Log messages, exception messages for developers and audit event names stay English and are not translated. |
| ENG-I18N-11 | **Translate into every supported language in the same change.** `Language.All` (`Domain/Localization/Language.cs`) is the list: English, Arabic and Urdu today. A new string gets its `ar` and `ur` text in the same PR; machine drafts are fine and are reviewed by a native speaker later (maintainer, 2026-11-12). Tests fail on a missing translation (ENG-I18N-60). |
| ENG-I18N-12 | **No concatenation or word-order assumptions.** Build a sentence as one message with named placeholders (`{0}` / `{PropertyName}` in resx, `${name}:NAME:` in `$localize`, `{{ name }}` inside an `i18n` element), never by joining fragments. Word order differs between languages. Do not reuse one string in two places where the grammar could differ. |
| ENG-I18N-13 | **Plurals and alternatives through ICU,** not `if count === 1`. Angular: `{count, plural, =0 {...} one {...} other {...}}` and `{value, select, ...}`. Give every category the target language needs (Arabic uses `zero`, `one`, `two`, `few`, `many`, `other`) and always `other`. |
| ENG-I18N-14 | **Context for translators.** Write clear, consistent source English, using the same term for the same thing everywhere (one glossary: school, branch, module, enrolment, grade). Add a meaning or description where a word is ambiguous (`i18n="button label|Saves the grade@@grades.save"`, a `<comment>` in resx). Name placeholders for what they hold. |
| ENG-I18N-15 | **No text in images or icons.** Icons carry an accessible label from the translations. Images, colours and icons must be easy to swap per market. |
| ENG-I18N-16 | **Plan for longer text.** Translations can be 30% or more longer than English, and Urdu needs more line height. Layouts must wrap and grow, with no fixed widths or heights on text and no truncation of meaning. Check each new screen in Arabic and Urdu. |

## 3. Backend (.NET)

| ID | Rule |
|---|---|
| ENG-I18N-20 | **Request culture comes only from `TenantRequestLocalization`.** Never set `CultureInfo.CurrentCulture` by hand in a handler, and never read `Accept-Language` yourself. The middleware picks the user's saved language, then this browser's cookie, then its languages, then the country default, limited to the school's languages. The culture is language plus country (`ur-PK`). |
| ENG-I18N-21 | **Server text in `.resx`.** Error titles live in `src/Web/Resources` (`IStringLocalizer<T>`). Validation messages and field names live in `src/Application/Common/Localization` (`ValidationMessages`, `PropertyNames`, strongly typed). Each neutral (English) `.resx` has `.ar.resx` and `.ur.resx` next to it. Add a field name to `PropertyNames` when a new validated property reaches users. |
| ENG-I18N-22 | **Lazy messages.** FluentValidation messages use `WithMessage(_ => ValidationMessages.X)` or `WithMessage(_ => string.Format(CultureInfo.CurrentCulture, ValidationMessages.X, value))`, never a string literal or an eagerly read property. A validator instance serves requests in different languages. A built-in message (FluentValidation's `NotEmpty`, `MaximumLength`, ...) that FluentValidation lacks in a portal language is added in `PortalLanguageManager`. |
| ENG-I18N-23 | **Culture-explicit formatting and parsing.** Text for people uses `CultureInfo.CurrentCulture`. Text for machines (logs, keys, file names, JSON you build by hand, protocol values, stored strings) uses `CultureInfo.InvariantCulture` or the round-trip format `"o"`. Always pass the culture or a `StringComparison` explicitly (analyzers CA1304, CA1305, CA1307, CA1309, CA1310, CA1311). Compare identifiers with `StringComparison.Ordinal` / `OrdinalIgnoreCase`, and change case with `ToUpperInvariant()` / `ToLowerInvariant()`. Sort text for display with the current culture's comparer. |
| ENG-I18N-24 | **Time zones.** Compute with UTC (`TimeProvider.GetUtcNow()`, ENG-ENTITY-08). When the server itself turns an instant into text or a calendar day (an export, an email, "today's classes"), convert with the caller's zone from `IRequestTimeZone`: the browser's `X-Time-Zone`, else the school's country, else UTC. Use IANA ids (`Asia/Karachi`) only, and `TimeZoneInfo.ConvertTime`. Never use the server's local time zone. |
| ENG-I18N-25 | **Languages and countries.** The supported languages are fixed in `Language.All`, and only languages with shipped translations go there. Countries, their languages, default language, time zone and currency are master-tenant data (`Country`). Never hard-code which language a school uses. |

## 4. Frontend (Angular)

| ID | Rule |
|---|---|
| ENG-I18N-30 | **Angular's own i18n only** (`@angular/localize`, runtime `loadTranslations` before bootstrap in `main.ts`; ADR-005). Do not add ngx-translate, Transloco or another i18n library. |
| ENG-I18N-31 | **Mark every template text with `i18n`** and every attribute a user reads (`aria-label`, `title`, `placeholder`, `alt`, `matTooltip`, component inputs such as `title` or `actionLabel`) with `i18n-<attribute>`. Use `<ng-container i18n>` when there is no element to mark. Mark code strings with `$localize`. |
| ENG-I18N-32 | **Always a custom id** in the form `@@area.key` (`@@grades.save`, `@@module.grades.name`), on `i18n`, `i18n-*` and `$localize` (`` $localize`:@@area.key:Text` ``). Keep an id when the meaning stays the same. Give the message a new id when its meaning changes, so the old translations are not reused by mistake. |
| ENG-I18N-33 | **Extract and translate:** run `npx ng extract-i18n` (writes `src/locale/messages.json`), then add every new id to `messages.ar.json` and `messages.ur.json`, appended so existing entries keep their order. Keep each `{$PLACEHOLDER}` exactly as it is. |
| ENG-I18N-34 | **Dates and times with the `localDate` pipe** (`core/i18n/local-date.pipe.ts`): page culture plus the user's time zone (device, else school). Do not use Angular's `date` pipe, `toLocaleString()` or `toLocaleDateString()` for display: the `date` pipe takes only fixed offsets, so it is wrong across daylight saving, and the others use the device's language instead of the page's. Numbers, percentages and currency use Angular's `number`, `percent` and `currency` pipes, which follow `LOCALE_ID` and `DEFAULT_CURRENCY_CODE`. |
| ENG-I18N-35 | **Server-owned names** that are fixed in code (modules, enum members) are translated in Angular by their key or name (`moduleText` pipe), with the server's text as the fallback. User-entered data (school, country and course names) is shown as entered and never translated. |
| ENG-I18N-36 | **Right to left.** `dir` and `lang` on `<html>` follow the language. Styles use logical properties only: `margin-inline-start`, `padding-inline-end`, `inset-inline-start`, `text-align: start`, `border-inline-*`. Never use `left`/`right` for layout. Icons that show direction (arrows, chevrons, "next") are mirrored in RTL; icons that don't (play, checkmark, logos) are not. Use the CDK `Directionality` in code that needs the direction. |
| ENG-I18N-37 | **Angular Material text:** provide translated intl classes (`MatPaginatorIntl` is done in `core/i18n/paginator-intl.ts`; add `MatDatepickerIntl`, `MatSortHeaderIntl` and similar when those components are first used) and set the date adapter's locale to the page culture. |
| ENG-I18N-38 | **Switching language** goes through `LocaleService.changeLanguage`, which saves the choice on the account and in this browser and reloads the page. Do not change `LOCALE_ID` or the translations at run time. |

## 5. Process and testing

| ID | Rule |
|---|---|
| ENG-I18N-50 | **Adding a language:** add its Angular translations (`messages.<code>.json`) and locale data (`LOCALE_DATA` in `app-locale.ts`), its `.resx` files for every neutral resource, FluentValidation built-ins if FluentValidation lacks the language, then an entry in `Language.All`. Then the master tenant can offer it per country. |
| ENG-I18N-51 | **Review in context.** Before a screen is done, open it in each language (dev seed: `demo.localhost` for Urdu, `uae.localhost` for Arabic) and check wrapping, direction, mirrored icons and number and date formats. Native-speaker review of the machine drafts is tracked separately and does not block a PR. |
| ENG-I18N-60 | **Tests.** `translations.spec.ts` (Angular) and `ValidationLocalizationTests` (backend) fail on a missing translation or a changed placeholder. Every new formatter or culture-dependent behaviour gets a test in at least two cultures, one of them right to left, and a time-zone test across a day boundary or daylight saving where it matters. A functional test that checks server text sets `Accept-Language`. |

The guard (`docs/ai/compliance-guard.py`) blocks the most common mistakes: a literal `WithMessage("...")`, `$localize` or `i18n` without a custom id, Angular's `date` pipe, and physical `left`/`right` CSS. Add an `engineering-exception: ENG-I18N-NN reason` comment only with the maintainer's approval.

## Why (sources)

- Microsoft's world-readiness guidance: design for i18n from the start, use Unicode, externalize strings, avoid concatenation, give translators context, use responsive layouts for longer text, respect locale conventions, store times as UTC, and test with pseudolocalization ([Software internationalization](https://learn.microsoft.com/globalization/methodology/software-internationalization); [Localization overview](https://learn.microsoft.com/globalization/localization/localization-overview)).
- Microsoft's engineering playbook: resource bundles, locale-aware libraries, automated tests across locales, localization in CI/CD, UTC internally, and locale formatting only on display ([Internationalization and localization](https://microsoft.github.io/code-with-engineering-playbook/non-functional-requirements/internationalization/)).
- .NET globalization: resource files, culture-explicit comparison, sorting and formatting, invariant persistence and round-trip formats, time zone conversion ([Globalization](https://learn.microsoft.com/dotnet/core/extensions/globalization); [Localization](https://learn.microsoft.com/dotnet/core/extensions/localization); [CA1305](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1305)). ASP.NET Core request localization and `IStringLocalizer` ([Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization)).
- Angular i18n: `i18n` attributes and `$localize`, meaning and description, custom ids, ICU `plural` and `select` with CLDR categories, extraction, locale data and `LOCALE_ID`, runtime translations ([Angular internationalization](https://angular.dev/guide/i18n)). Practical advice on custom ids, translator descriptions, placeholders, ICU plurals and locale-aware pipes ([POEditor: Angular internationalization and localization](https://poeditor.com/blog/angular-internationalization-localization/)).
- Plural categories per language: [CLDR language plural rules](https://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html). Logical CSS properties: [CSS logical properties](https://www.w3.org/TR/css-logical-1/).

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft Globalization documentation: software internationalization and localization overview](https://learn.microsoft.com/globalization/) | Microsoft Learn Globalization docs (current on Learn) | Automatic: new commits in `MicrosoftDocs/globalization/globalization` | 2026-11-12 |
| [Microsoft Code With Engineering Playbook: internationalization and localization](https://microsoft.github.io/code-with-engineering-playbook/non-functional-requirements/internationalization/) | Engineering playbook (main) | Automatic: new commits in `microsoft/code-with-engineering-playbook/docs/non-functional-requirements/internationalization.md` | 2026-11-12 |
| [.NET globalization and localization (resources, culture-explicit formatting and comparison, time zones)](https://learn.microsoft.com/dotnet/core/extensions/globalization) | .NET 10 | Automatic: new commits in `dotnet/docs/docs/core/extensions/globalization.md` | 2026-11-12 |
| [Globalization and localization in ASP.NET Core (request localization, IStringLocalizer)](https://learn.microsoft.com/aspnet/core/fundamentals/localization) | ASP.NET Core 10 | Automatic: new commits in `dotnet/AspNetCore.Docs/aspnetcore/fundamentals/localization` | 2026-11-12 |
| [Angular internationalization guide (i18n, $localize, ICU, custom ids, locale data)](https://angular.dev/guide/i18n) | Angular v22 guide (adev/src/content/guide/i18n) | Automatic: new commits in `angular/angular/adev/src/content/guide/i18n` | 2026-11-12 |
| [POEditor: Angular internationalization and localization best practices](https://poeditor.com/blog/angular-internationalization-localization/) | Article as read on 2026-11-12 | Manual: listed for a portal check | 2026-11-12 |
<!-- END GENERATED: sources -->
