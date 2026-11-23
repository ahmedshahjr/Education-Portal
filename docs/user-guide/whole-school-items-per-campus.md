# Whole-school items per campus

**Previous:** [Create a user](create-a-user.md). **Next:** none yet. This is the last page of the first guide.

## What this does

A **whole-school** item applies to every campus of the school: a holiday, a calendar event or a school photo. The portal saves one copy per campus, so each campus shows the item in its own calendar and gallery. A campus added later also gets a copy of each whole-school item the school already has.

Every other calendar item belongs to one campus only, and shows only there.

## Who can do it

A **school administrator**, with the **Academic setup** module on, and the right to create (or edit or delete) on every campus the item applies to.

## Before you start

- The school has its campuses (see [Add a campus](add-a-campus.md)).
- The **Academic setup** module is on for the school (see [Enable modules for a school](enable-modules-for-a-school.md)).
- You have the rights on every campus. A whole-school item is refused if you lack the right on any campus.

## Steps

1. Sign in as the school administrator.
2. In the menu, open the **Academic calendar** page.
3. Open the tab for holidays or calendar events.
4. Select the add button for a holiday or an event.
5. Type the title, the start and end dates, and the other details shown on the form.
6. In the **Campus** field, choose **Whole school** to apply the item to every campus, or choose one campus.
7. Save the item.
8. The item appears once in the school's calendar view, and on every campus.

To change or delete a whole-school item, open it: the change or delete applies to every copy. An item that belongs to one campus cannot be made whole-school later, and the other way round, by an edit.

## What each right allows

The right to create, edit or delete is checked on every campus of the item. If you have the right on three campuses and not the fourth, you cannot save a whole-school item.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| 403 Forbidden | You lack the right on one of the campuses, or the module is off. | Ask the master tenant administrator or your school administrator to give the right on that campus. |
| A campus field cannot be changed to *Whole school* on an existing item | Not allowed: the campus of an item cannot change by an edit. | Delete the item and add it again as whole-school. |
| *A holiday must be under 120 days long.* | The dates are too far apart. | Shorten the holiday. |
| *The end date cannot be before the start date.* | Dates are reversed. | Correct the dates. |

## Worked example

The school administrator adds a holiday *Example Eid break* from 1 to 3 of a month, campus *Whole school*. Each campus gets its own copy. The school calendar shows the holiday once. A new campus *Example North Campus* is added the following week and automatically gets the holiday too.

## Flow recap

*Campuses exist, then Academic calendar, then add a holiday or event with Whole school as the campus, then save; every campus gets a copy, and a new campus gets the copies too.*

**Not verified in the running app:** the names of the tabs on the Academic calendar page and whether the calendar view shows the item once. The behaviour is taken from the code and the decision record ADR-027.
