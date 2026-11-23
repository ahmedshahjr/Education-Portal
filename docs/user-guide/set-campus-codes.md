# Set campus codes

**Previous:** [Add a campus](add-a-campus.md). **Next:** [Create a role](create-a-role.md).

## What this does

Sets the code shown in front of every campus of your school, for example `KHI0002` when the prefix is `KHI`. A code is a four-digit number with an optional prefix.

## Who can do it

A **school administrator**, with the **Campuses** module on.

## Before you start

- At least one campus exists (see [Add a campus](add-a-campus.md)).
- Decide the optional **prefix** for the school, for example `KHI`.

## Steps

1. Sign in as the school administrator.
2. In the menu, select **Customization**, then **Campus codes**.
3. Type the **prefix**, if the school uses one.
4. Check the code shown for each campus. A code has four digits.
5. Select **Save**.
6. You see *Campus codes saved.*

## What each right allows

Only the school administrator changes codes. Codes are shown on every campus of the school.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| Validation message on the code | The code is not four digits, or the prefix is too long. | Correct it. The exact limit is set by the server; the screen shows it. |
| 403 Forbidden | You are not the school administrator, or the module is off. | See [Enable modules for a school](enable-modules-for-a-school.md). |

## Worked example

The school sets the prefix `KHI`. The campus with code `0002` is shown as `KHI0002`.

## Flow recap

*Campuses exist, then Customization, Campus codes, prefix and four-digit codes, then Save.*

**Not verified in the running app:** the exact validation limits of the prefix; the page wording is taken from the translation file.
