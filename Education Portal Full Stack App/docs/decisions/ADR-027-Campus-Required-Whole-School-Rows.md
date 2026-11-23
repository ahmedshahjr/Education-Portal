# ADR-027: Campus Required, One Row per Campus for Whole-School Items

## Status

Accepted on 2026-11-23. It refines [ADR-026](ADR-026-Row-Level-Security-And-Tenant-Column.md) for the campus column.

## Date

2026-11-23

## Context

Eight school tables had an optional `CampusId`: a `null` meant "the whole school". Holidays, calendar events, school photos,
class offerings, courses, timetable slots, admission enquiries and school reviews were all written that way. The
optional campus caused three problems:

1. A row with no campus is not visible to a person who only has rights on one campus. Every check had to remember a
   separate "whole school" branch, and the checks could be skipped by mistake (`if (campusId is { } id)`).
2. A whole-school row could not be filtered by campus in SQL without an extra `OR ... IS NULL`, so the campus filter and
   the query index did not match.
3. A campus added after a whole-school holiday had no entry of its own, so the campus's calendar and gallery did not
   show what the school shows.

## Options

**A. Keep the optional campus.** Keeps the "whole school" branch in every check and query. Rejected.

**B. Required campus on every row. A whole-school item becomes one row per campus, sharing a key (chosen).** A campus is
always present, so every check is the same. The copies share a `WholeSchoolKey` (`uuid`, nullable). Saving a
whole-school item checks the right on every campus. Changing or deleting one copy changes or deletes every copy with the
same key. A campus added later receives a copy of each key the school has, in the same transaction as the campus.

**C. A separate table for whole-school items.** Two tables to keep in step, and every list query would union them. Rejected.

## Decision (option B)

- **Entities.** `ClassOffering`, `Course`, `TimetableSlot`, `AdmissionEnquiry` and `SchoolReview` take a required campus
  (`int`). `Holiday`, `CalendarEvent` and `SchoolPhoto` also take a required campus, and each has `WholeSchoolKey`.
  `WholeSchoolKey` is `null` for an item of one campus.
- **Create.** A holiday, event or photo with no campus is saved once per campus of the school, all with one new key
  (`Guid.CreateVersion7()`). A school without a campus is refused (400). Each campus is checked with `ICampusAccess`
  before anything is saved, and the request is refused if the user lacks the right on any of them.
- **Change and delete.** A whole-school item changes or is deleted for every row with its key. The campus of an item
  never changes: a single-campus item cannot be made whole-school, and the reverse, by an update (400).
- **Reads.** The school calendar and the photo gallery show a whole-school item once, with no campus. Its first visible copy
  stands for it.
- **New campus.** `CreateCampus` copies each whole-school holiday, event and photo of the school to the new campus, under the
  same key, and saves the campus and the copies in one transaction (`ExecuteInTransactionAsync`).
- **Migration.** `WholeSchoolRowPerCampus` adds `WholeSchoolKey`, then gives each existing whole-school row a key derived
  from its id, copies it to every campus of its school and deletes the original. It stops with an error, changing nothing
  else, when a row cannot be given a campus (a school with no campus, or a per-campus table with a row without a campus).
  It runs with the row-level security bypass for its own transaction only (ADR-026).

## Consequences

- Holiday, event and photo lists grow by one row per campus in the database. The copies are small; photos are copied
  with their bytes.
- A whole-school photo is uploaded once per campus. Replacing a photo replaces every copy.
- Clients must send a campus for class offerings, courses and timetable slots. Without one the API answers 400.
- A later change that needs per-campus differences for a whole-school item must split the copy; it cannot be stored as one
  shared row.

## Tests

`WholeSchoolRowsTests` (functional) covers the fan-out for holidays and events, the change and delete of every copy, a
single-campus holiday that cannot become whole-school, and the copies for a new campus. `CampusAccessTests` covers a
person refused a whole-school holiday because they lack the right on one campus. Domain tests cover the required campus
and `JoinWholeSchool`.
