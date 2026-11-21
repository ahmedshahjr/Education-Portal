# ADR-023: Readable Change History Shown by Permission

## Status

Accepted by the maintainer on 2026-11-21. Amends [ADR-016](ADR-016-Change-Audit-For-Commercial-Records.md).

## Context

`ChangeAuditEntry` records who (user id), when and old and new values, but only for commercial records, only the master tenant can
read them, and the values are raw property names. Schools need to see who changed a campus, role or person, in words.

## Decision

1. **More audited types:** `SchoolRole`, `RolePermission`, `Campus` and `UserDetails` join `ChangeAuditInterceptor.AuditedTypes`.
   Values that are personal data (phone, address, email, birth date) are recorded as "changed" without the value
   (SEC-PII-02), so the history says that a phone number changed, never what it was.
2. **Read API:** `GET /api/ChangeAudit` gets paging and filters (entity, user, date range, module). It needs the
   `ViewHistory` right (ADR-022) in the entry's module and campus; the tenant filter of ADR-016 stays, so a school
   reads only its own entries.
3. **Sentences, in the reader's app:** the sentence is built in Angular from the entry's type, action and values, with
   `$localize` in English, Arabic and Urdu (`audit.page.ts`). This replaces the server-side `IStringLocalizer` mapper
   planned above: the wording lives with the other screen text, and the server only sends data. Times are sent as UTC
   and shown in the user's time zone. The actor's sign-in name is read at query time (`GetUserNameAsync`), not stored
   in the entry, and is sent with each row.
4. **Angular:** the history page shows filters, paging and the sentences, and appears only for users with `ViewHistory`
   (rights guard).

## Consequences

- Easier: schools can answer "who changed this" without the master tenant.
- Harder: each audited entity needs a module and a label for its sentence; a new entity without one is not shown.
- Retention is still undecided (TODO from ADR-016).
