# ADR-020: Self-Service School Registration and the School Without a Domain

- **Status:** Accepted
- **Date:** 2026-11-21
- **Supersedes in part:** ADR-017 (a school exists only after the master approves its request; the trial is 30 days)

## Context

Until now a school could only ask to join (the "request a school" form) and wait for the master tenant to approve it. The
maintainer wants a school to register itself: one form with its name, country and the administrator's email and password,
and a working school with a free trial at once, with no waiting. The master tenant still wants to see which schools are on
a trial and which pay, and a school without a domain of its own must be able to sign in.

## Decision

1. **Self-service registration.** `POST /api/SchoolRegistrations` (`RegisterSchoolCommand`) is open without sign-in. It takes
   the school name, the country, the administrator's email, the password and the password typed again. In one transaction
   (`ExecuteInTransactionAsync`) it runs the same `SchoolRegistration.RegisterAsync` as approval and as the master adding a
   school: the school, the modules that are on by default, the trial and the administrator. The administrator chose the
   password, so the account is **not** forced to change it (`MustChangePassword` is false). The master's one-time passwords
   are unchanged.
2. **Trial of three months.** The trial is 90 days (`SchoolRegistration.TrialDays`, `Subscription.MaxTrialDays`), which is the
   "3 months" the maintainer asked for. ADR-017 said 30 days in its text only; the code already used 90.
3. **No domain at first.** A self-registered school has no domain. It signs in on the master tenant's domain and acts as its
   own school there (item 5 below). The master can give it a domain later; its session on the master domain then ends and
   it signs in on its own domain. Registration is accepted on the master tenant's domain only (404 on any other host).
4. **Countries.** `GET /api/SchoolRegistrations/countries` lists, without sign-in and on the master domain only, the countries
   that have a price book (public id and name), because the trial is priced from the price book.
5. **A school without a domain signs in on the master domain** (built earlier, documented here). The account is found by its
   email (unique across the platform), the session carries the `hosted_on_master` claim, and the session check switches the
   request to the account's school with master power removed. The sign-in answer lists the school's modules, not the master's,
   and changing the password keeps the claim, so the session survives the first password change.
6. **The request queue stays.** The "request a school" form and the master's approval queue remain as a second path for
   schools that prefer to ask first. Approval still sets the domain and registers the school.
7. **Trial status in the master portal.** The school list shows each school's plan: Trial (with the days left), Paid (an
   active subscription), payment overdue, suspended or no plan (`SchoolSummaryDto.SubscriptionStatus` and `TrialEndDate`).
8. **Trial banner.** A school's sign-in page on its own domain shows a banner with the trial's days left, from
   `GET /api/Subscriptions/trial-notice`, which is open without sign-in and carries only a flag and a day count for the school
   of the host. A school without a domain sees the banner in the app after it signs in (the existing banner reads
   `GET /api/Subscriptions/mine`). The notice is **not** looked up by email before sign-in: that would reveal whether an email
   has an account and whether its school is on a trial (SEC-AUTHN-05).

## Consequences

- Anyone can create a trial school: there is no email verification yet (email sending is not built). The mitigation is the
  per-address rate limit of the school-request policy (`RateLimiting:SchoolRequestPermitLimit`, default 5 an hour). A
  verification step is a follow-up before this is opened to the public internet.
- "Another account already uses this email" on an anonymous endpoint tells a visitor that an email has an account. The rate
  limit bounds how fast that can be probed; accepted for now and noted here.
- The countries list and the trial notice are public by design and carry no personal data (compliance exceptions SEC-AUTHN-01
  are written at the endpoints).
- Self-registration, the trial notice and the sign-in answer are covered by functional tests (`RegisterSchoolTests`,
  `SchoolPlanTests`, `SchoolWithoutDomainTests`).
- A later `Tenant` above `School` (a tenant that owns schools) is not part of this decision; it gets its own ADR.
