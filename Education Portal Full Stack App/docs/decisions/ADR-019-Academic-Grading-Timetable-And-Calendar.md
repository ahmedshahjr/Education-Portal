# ADR-019: Academic Foundations, Grading, Timetable Clash Checks and Calendar

- **Status:** Accepted, not yet compiled or run
- **Date:** 2026-11-19
- **Builds on:** ADR-018 (role rights), ADR-016 (change audit), ADR-008 (public ids and enums), ADR-005 (localization)

## Context

The school needs classes taught by teachers, students enrolled in them, graded assessments, report cards with a grade
letter and GPA, timetables without clashes, and a calendar that notifies the people it concerns. Only the class offering
existed. Nothing in the academic model was checked against roles or teachers.

## Decision

1. **Minimal academic entities** in the `academics` schema, all `ITenantOwned`: `Course` (a class offering, a teacher
   and an optional campus), `Enrolment` (a student in a class once), `Assessment` (a weight from 0 to 100 in its course
   and a maximum score), `Score` (one per student and assessment, at most the maximum), `TimetableSlot` (a course, a day,
   a time, a room and a campus), and `GradingScale` with `GradingBand` rows.
2. **Teachers grade their own courses.** Recording a score or adding an assessment requires the signed-in account to
   belong to the course's teacher. Administrators are not limited by this rule (`CourseOwnership`).
3. **Assessment weights** of a course cannot add up to more than 100. A course percent is the weighted average of the
   recorded scores, with only the assessments that have a score counted. A course with no recorded score has no percent.
4. **Grading scales** are validated by `GradeScale.Validate`: bands cover 0 to 100 once, with no gap and no overlap, a
   letter of up to 5 characters, and non-negative points. The GPA is the mean of the grade points of the graded courses,
   rounded half up to 2 decimals.
5. **Who reads a report card.** An administrator, the student, and a parent linked to the student see every course. A
   teacher sees only the lines of the courses they teach in that class. Anyone else is refused.
6. **Timetable clash rules** (`TimetableConflicts`): two lessons clash when they are on the same day, their times overlap
   (touching at an end is not a clash), and they share the teacher, the room on the same campus, or the class offering.
   A clash refuses the create or move, naming the lessons by public id. A preview endpoint runs the same check without saving.
7. **Calendar and notifications.** A school event has an audience (All, Staff, Parents or Students). Staff are every person
   who is not a student or a parent. An account with no person record reaches only All. Creating an event writes one
   notification per recipient in the same save. A notification belongs to its recipient, and marking it read is refused
   as not found for any other account (IDOR).
8. **Modules and rights.** Academic writes need the Academic setup module (courses, enrolments) or the Exams module
   (assessments, scores, grading scales, report cards); the timetable needs the Timetable module; the calendar needs the
   School module, a core module. Each use case declares its right (`Create`, `Edit`, `View`) as ADR-018 describes.
9. **Audit.** Course, enrolment, assessment, score, grading scale, grading band, timetable slot and school event changes
   are written to `ChangeAuditEntries`. Notifications are not audited, because one event creates many of them.

## Consequences

- Hand-written migrations `AddAcademicFoundations`, `AddGradingScales` and `AddCalendarAndNotifications`. The model
  snapshot is not updated for these tables. Run `dotnet ef migrations list` and add the snapshot before merging.
- Nothing in this change has been compiled or run. The Application, Infrastructure and Web projects, the unit tests and
  the migrations are unverified in the environment that wrote them.
- The functional tests required for each use case (another school, another campus, a disabled module, a missing right)
  are not written yet. They must be added before merge.
- A course cannot change its class or teacher once created; there is no update endpoint for courses yet.
- A timetable slot can be moved but not reassigned to another course.
- Campus-scoped events are not built. Events are school-wide.
- A notification keeps the text as the administrator wrote it, in the language they typed. Translating it per reader needs
  a message-key design, not yet decided.
- An event for a large audience creates one row per recipient in one save. Batching is needed before a large school uses it.
- The conflict check loads the school's lessons on the day into memory. This is fine for one school's timetable; it
  needs a range query if it grows.

## Alternatives considered

- **Grades stored as one final grade per course.** Rejected: AGENTS.md section 6 requires several scored assessments per
  enrolment.
- **Owned collection for grading bands.** Rejected: separate tenant-owned rows keep the isolation tests and the
  `DatabaseSchemas` mapping uniform.
- **Email and SMS for notifications.** Rejected for now: in-app only, as the maintainer decided. The notification row is
  the place an email sender would read from later.
