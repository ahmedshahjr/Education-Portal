---
name: angular-material-ui
description: Angular 22 frontend rules following the official Angular best practices and style guide and Google's Material Design 3 UI/UX guidelines through Angular Material and the CDK (standalone components, signals, native control flow, Signal Forms, lazy routes, theming with design tokens, layout, feedback, accessibility WCAG 2.2 AA, testing, linting). Use when changing anything in src/Web/ClientApp. Part of the mandatory Education Portal engineering rules.
---

# Angular and Material Design UI/UX (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

Based on the Angular team's [AI guidelines](https://angular.dev/ai/develop-with-ai) and [agent skills](https://angular.dev/ai/agent-skills), the [Angular style guide](https://angular.dev/style-guide), [Angular accessibility](https://angular.dev/best-practices/a11y), [Angular Material](https://material.angular.dev/) and its [theming guide](https://material.angular.dev/guide/theming), and Google's [Material Design 3](https://m3.material.io/) guidelines. The Angular team's own skill (`angular-developer` in [angular/skills](https://github.com/angular/skills)) goes deeper on each API; when it and this file differ, this file wins for this repo and the difference is raised in a PR. Security rules for the frontend (no `innerHTML`, no sanitizer bypass, antiforgery) are in the [compliance skills](../../secure-compliance/SKILL.md) and take precedence.

## Angular code

| ID | Rule |
|---|---|
| ENG-UI-01 | **Standalone only.** New components, directives and pipes are standalone (do not write `standalone: true`, it is the default). No new `NgModule`s. Import only the directives and pipes a template uses; never `CommonModule`. |
| ENG-UI-02 | **Signals for state.** Use `signal()` for local state, `computed()` for derived state, `linkedSignal()` for state reset by another source, `input()`/`output()`/`model()` instead of `@Input`/`@Output`, and `set`/`update` (never mutation). Use `resource`/`httpResource` or RxJS with the `async` pipe or `toSignal` for async data; no manual `subscribe` without `takeUntilDestroyed`. |
| ENG-UI-03 | **Native control flow.** Use `@if`, `@for` (always with `track`), `@switch` and `@defer`; never `*ngIf`, `*ngFor`, `*ngSwitch`. Use `class`/`style` bindings, not `ngClass`/`ngStyle`. Keep logic out of templates: no method calls with work in bindings, use `computed()` or pipes. |
| ENG-UI-04 | **Change detection.** Rely on `OnPush` (the default in Angular 22); do not write code that depends on Zone.js to notice mutations. |
| ENG-UI-05 | **Dependency injection.** Use `inject()`, not constructor parameters. Services have one responsibility; app-wide singletons use the Angular 22 `@Service` decorator (or `@Injectable({ providedIn: 'root' })`). HTTP calls go through the NSwag-generated client (`web-api-client.ts`); never edit the generated file. |
| ENG-UI-06 | **Forms.** New forms use Signal Forms (`@angular/forms/signals`); otherwise reactive forms. No template-driven forms. Client validation mirrors, but never replaces, the server's FluentValidation rules (SEC-INPUT-01); show the server's ProblemDetails field errors next to the fields. |
| ENG-UI-07 | **Structure and routing.** Feature folders under `src/app/<feature>/`, file names per the Angular style guide (`student-list.ts`, `student-list.html`, `student-list.spec.ts`; one concept per file). Feature routes are lazy-loaded with `loadComponent`/`loadChildren`; guards are functional (`CanActivateFn`). Branch-scoped pages live under `/branches/:branchId/...` (AGENTS.md section 2). Strict TypeScript; no `any` (use `unknown` and narrow). |

## Project theme (`Theme Angular/`)

| ID | Rule |
|---|---|
| ENG-UI-17 | **Use the project theme as the guide** (maintainer rule, 2026-11-10). The maintainer's Angular Material theme lives in [`Theme Angular/`](../../../../Theme%20Angular/) at the repo root and contains ready-made, built-in components for every common screen (layout shell, sidebar, top bar, customizer, cards, tables, forms, dialogs, auth pages, settings, dashboards, charts, UI showcase in `src/app/features/ui-showcase`). Before building any screen or component in `src/Web/ClientApp`, look in the theme first and reuse its component, styles (`src/styles/`) and `--mm-*` design tokens, adapting it to these rules (standalone, signals, generated API client, security rules) rather than writing a new one. Copy only what a change needs into `src/Web/ClientApp`; never import from `Theme Angular/` at build time, and never edit it to fit one feature. If the theme has nothing that fits, build on Angular Material and the CDK (ENG-UI-08) in the theme's visual style, and say so in the PR. Per-school branding (preset, sidebar colour, logo, favicon) only changes the theme's tokens through `BrandingService`; components never hard-code a school's colours. |

## Material Design 3 UI/UX

| ID | Rule |
|---|---|
| ENG-UI-08 | **Angular Material + CDK** are the UI component library, following Material Design 3. Use a Material component when one exists (buttons, form fields, tables with sort and paginator, dialogs, snack bars, menus, tabs, sidenav, toolbar, chips, progress indicators) before writing a custom one. Build custom components on the CDK (overlay, a11y, drag-drop, layout) and style them with Material system tokens. Before either, check the project theme for a ready-made component (ENG-UI-17). |
| ENG-UI-09 | **Theming with tokens.** One Material 3 theme defined with the `mat.theme(...)` Sass mixin (colour, typography, density), with light and dark variants. Components use the system CSS variables (`--mat-sys-primary`, `--mat-sys-surface`, `--mat-sys-body-medium`, ...), the project theme's `--mm-*` tokens (ENG-UI-17) and Material's override mixins. No hard-coded colours, fonts or shadows in component styles, no `::ng-deep`, and no overriding Material's internal class names. |
| ENG-UI-10 | **Layout.** Responsive with Material's window size classes (compact under 600px, medium 600 to 839px, expanded 840px and up) using the CDK `BreakpointObserver` or CSS container/media queries. Navigation follows Material patterns: navigation drawer on expanded screens, navigation rail on medium, a modal drawer or bottom navigation on compact. Use the 4dp/8dp spacing grid. Every page works from 360px wide. |
| ENG-UI-11 | **Feedback and states.** Every data view has loading (progress indicator or skeleton), empty (explanation and next action) and error (plain-language message and retry) states. Confirm destructive actions in a dialog that names what will be deleted; acknowledge completed actions with a snack bar; never block the whole screen for a background save. |
| ENG-UI-12 | **Content and forms UX.** Use sentence case labels, one primary action per view (filled button), secondary actions as outlined or text buttons, `mat-form-field` with a visible label (not placeholder-only), helper and error text under the field, and errors shown after the user leaves the field or submits. Tables and lists show which branch and school the user is in. |
| ENG-UI-13 | **Accessibility (WCAG 2.2 AA).** Every page passes axe checks: semantic HTML first, labels for every control, `alt` text, visible focus, logical focus order and focus moved to dialog content and back on close (CDK `A11yModule`/`FocusTrap`), colour contrast at least 4.5:1 for text (Material 3 tokens give this when not overridden), touch targets at least 48 by 48 px, no information conveyed by colour alone, and status changes announced with `LiveAnnouncer`. Use `NgOptimizedImage` for static images. Also see the EU accessibility rules in `standard-eu`. |

## Quality

| ID | Rule |
|---|---|
| ENG-UI-14 | **Lint and format.** Code passes `ng lint` (`angular-eslint` recommended and template accessibility rule sets) once it is added (overview section 6), and is formatted by the repo's Prettier/EditorConfig settings. |
| ENG-UI-15 | **Tests.** Component and service tests use Vitest with `TestBed`, test behaviour through the rendered DOM and component harnesses (`MatButtonHarness`, `MatTableHarness`), not private fields. Every new component has at least one test; user journeys that cross the API are covered by `Web.AcceptanceTests` (Playwright). |
| ENG-UI-16 | **Performance.** Respect the production budgets in `angular.json`; use `@defer` for heavy below-the-fold widgets (charts) and lazy routes for features. No new global CSS frameworks next to Angular Material. |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Angular team's AI rules for coding agents (guidelines.md, AGENTS.md, GEMINI.md)](https://angular.dev/ai/develop-with-ai) | Angular v22 guidelines (adev/src/context) | Automatic: new commits in `angular/angular/adev/src/context` | 2026-11-10 |
| [Official Angular agent skills (angular-developer)](https://angular.dev/ai/agent-skills) | angular/skills, built 2026-11-10 from angular/angular 0dafb36 | Automatic: new commits in `angular/skills/angular-developer` | 2026-11-10 |
| [Angular style guide and best practices (incl. accessibility)](https://angular.dev/style-guide) | angular.dev best practices for Angular v22 | Automatic: new commits in `angular/angular/adev/src/content/best-practices` | 2026-11-10 |
| [Angular Material and CDK guides (Material 3 theming)](https://material.angular.dev/guide/theming) | Angular Material 22 (Material 3 theming with mat.theme and system tokens) | Automatic: new commits in `angular/components/guides` | 2026-11-10 |
| [Google Material Design 3 guidelines (layout, components, accessibility)](https://m3.material.io/) | Material Design 3 | Manual: listed for a portal check | 2026-11-10 |
| [W3C Web Content Accessibility Guidelines](https://www.w3.org/TR/WCAG22/) | WCAG 2.2 Level AA | Manual: listed for a portal check | 2026-11-10 |
<!-- END GENERATED: sources -->
